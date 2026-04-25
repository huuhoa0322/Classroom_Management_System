using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

public class StudentPackageRepository : IStudentPackageRepository
{
    private readonly AppDbContext _ctx;

    public StudentPackageRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<StudentPackage?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.StudentPackages.FirstOrDefaultAsync(sp => sp.Id == id, ct);

    public async Task<IEnumerable<StudentPackage>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.StudentPackages.AsNoTracking().ToListAsync(ct);

    public async Task<List<StudentPackage>> GetByStudentIdAsync(int studentId, CancellationToken ct = default)
        => await _ctx.StudentPackages
            .AsNoTracking()
            .Include(sp => sp.Package)
            .Where(sp => sp.StudentId == studentId)
            .OrderByDescending(sp => sp.CreatedAt)
            .ToListAsync(ct);

    public async Task<StudentPackage?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await _ctx.StudentPackages
            .Include(sp => sp.Package)
            .Include(sp => sp.Student)
            .FirstOrDefaultAsync(sp => sp.Id == id, ct);

    public async Task AddAsync(StudentPackage entity, CancellationToken ct = default)
        => await _ctx.StudentPackages.AddAsync(entity, ct);

    public void Update(StudentPackage entity)
        => _ctx.StudentPackages.Update(entity);

    public void Delete(StudentPackage entity)
    {
        entity.IsDeleted = true;
        _ctx.StudentPackages.Update(entity);
    }

    public async Task<List<StudentPackage>> GetActiveWithDetailsAsync(CancellationToken ct = default)
        => await _ctx.StudentPackages
            .AsNoTracking()
            .Include(sp => sp.Package)
            .Include(sp => sp.Student)
                .ThenInclude(s => s.Parent)
            .Where(sp => sp.Status == "active")
            .ToListAsync(ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
