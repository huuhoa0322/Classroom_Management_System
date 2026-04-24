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
