using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _ctx;

    public StudentRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Student?> GetByIdAsync(int id, CancellationToken ct = default)
        // FindAsync bỏ qua HasQueryFilter (soft-delete) — dùng FirstOrDefaultAsync thay thế
        => await _ctx.Students.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IEnumerable<Student>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Students.AsNoTracking().ToListAsync(ct);

    public async Task<(List<Student> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? statusFilter, CancellationToken ct = default)
    {
        var query = _ctx.Students
            .AsNoTracking()
            .Include(s => s.Parent)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter))
            query = query.Where(s => s.Status == statusFilter);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.EnrolledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Student?> GetWithParentAsync(int id, CancellationToken ct = default)
        => await _ctx.Students
            .AsNoTracking()
            .Include(s => s.Parent)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(Student entity, CancellationToken ct = default)
        => await _ctx.Students.AddAsync(entity, ct);

    public void Update(Student entity)
        => _ctx.Students.Update(entity);

    public void Delete(Student entity)
    {
        entity.IsDeleted = true;
        _ctx.Students.Update(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
