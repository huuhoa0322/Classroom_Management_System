using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CLS.DAL.Configurations;

/// <summary>
/// Abstract Fluent API configuration chung cho mọi Entity kế thừa BaseEntity.
///
/// Tự động áp dụng:
/// - HasKey(Id) với ValueGeneratedOnAdd (PostgreSQL IDENTITY)
/// - Column mapping sang snake_case (PostgreSQL convention)
/// - HasQueryFilter để ẩn soft-deleted records khỏi mọi query
///
/// CÁCH DÙNG: Mỗi Entity Configuration kế thừa class này, gọi base.Configure(builder)
/// rồi thêm cấu hình riêng của Entity đó.
/// </summary>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // ── Primary Key ───────────────────────────────────────────────────────
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasColumnName("id")
               .ValueGeneratedOnAdd();     // PostgreSQL IDENTITY / SERIAL

        // ── Audit Fields ──────────────────────────────────────────────────────
        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(x => x.UpdatedAt)
               .HasColumnName("updated_at")
               .IsRequired();

        // ── Soft Delete ───────────────────────────────────────────────────────
        builder.Property(x => x.IsDeleted)
               .HasColumnName("is_deleted")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(x => x.DeletedAt)
               .HasColumnName("deleted_at")
               .IsRequired(false);

        // Global soft-delete filter — ẩn tất cả record có IsDeleted = true
        // khỏi mọi query (trừ khi dùng IgnoreQueryFilters())
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
