using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

public class ClassRepository : IClassRepository
{
    private readonly AppDbContext _ctx;

    public ClassRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Class?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.Classes.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IEnumerable<Class>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Classes.AsNoTracking().ToListAsync(ct);

    public async Task<List<Class>> GetAllActiveAsync(CancellationToken ct = default)
        => await _ctx.Classes
            .AsNoTracking()
            .Where(c => c.Status == "active")
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<(List<Class> Items, int Total)> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _ctx.Classes
            .AsNoTracking()
            .Include(c => c.ClassStudents)
            .Include(c => c.Sessions)
            .OrderBy(c => c.Name);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Class?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await _ctx.Classes
            .Include(c => c.ClassStudents)
                .ThenInclude(cs => cs.Student)
            .Include(c => c.Sessions)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        var query = _ctx.Classes.Where(c => c.Name == name);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }


    public async Task AddAsync(Class entity, CancellationToken ct = default)
        => await _ctx.Classes.AddAsync(entity, ct);

    public void Update(Class entity) => _ctx.Classes.Update(entity);

    public void Delete(Class entity)
    {
        entity.IsDeleted = true;
        _ctx.Classes.Update(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
