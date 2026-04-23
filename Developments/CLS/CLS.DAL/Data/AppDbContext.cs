using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Data;

/// <summary>
/// EF Core DbContext cho CLS — Data Access Layer.
///
/// Trách nhiệm:
/// 1. Đăng ký tất cả DbSet (thêm dần theo từng Vertical Slice P7+)
/// 2. Apply toàn bộ IEntityTypeConfiguration từ assembly (OnModelCreating)
/// 3. Tự động set CreatedAt / UpdatedAt khi SaveChangesAsync (audit trail)
/// 4. Tự động set DeletedAt khi soft-delete
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── DbSets — thêm dần theo từng Vertical Slice (P7 trở đi) ──────────────
    public DbSet<User> Users => Set<User>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<Student> Students => Set<Student>();

    // ── Financial Administration (CLS-003) ────────────────────────────────────
    public DbSet<TuitionPackage> TuitionPackages => Set<TuitionPackage>();
    public DbSet<StudentPackage> StudentPackages => Set<StudentPackage>();
    public DbSet<Payment> Payments => Set<Payment>();

    // ─────────────────────────────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply tất cả IEntityTypeConfiguration<T> được định nghĩa trong CLS.DAL assembly
        // Không cần đăng ký thủ công từng Configuration — tự động scan
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Override SaveChangesAsync để tự động set audit fields (CreatedAt, UpdatedAt)
    /// và DeletedAt khi soft-delete — Service layer KHÔNG cần set thủ công.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;

                    // Nếu đang soft-delete (IsDeleted vừa được set = true), ghi lại timestamp
                    if (entry.Entity.IsDeleted && entry.Entity.DeletedAt is null)
                        entry.Entity.DeletedAt = now;

                    // Không cho phép thay đổi CreatedAt sau khi đã tạo
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
