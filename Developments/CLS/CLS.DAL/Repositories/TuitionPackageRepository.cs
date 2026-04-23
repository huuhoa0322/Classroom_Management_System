using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

public class TuitionPackageRepository : ITuitionPackageRepository
{
    private readonly AppDbContext _ctx;

    public TuitionPackageRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<TuitionPackage?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.TuitionPackages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IEnumerable<TuitionPackage>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.TuitionPackages.AsNoTracking().ToListAsync(ct);

    public async Task<List<TuitionPackage>> GetAllActiveAsync(CancellationToken ct = default)
        => await _ctx.TuitionPackages
            .AsNoTracking()
            .Where(p => p.Status == "active")
            .OrderBy(p => p.TotalSessions)
            .ToListAsync(ct);

    public async Task AddAsync(TuitionPackage entity, CancellationToken ct = default)
        => await _ctx.TuitionPackages.AddAsync(entity, ct);

    public void Update(TuitionPackage entity)
        => _ctx.TuitionPackages.Update(entity);

    public void Delete(TuitionPackage entity)
    {
        entity.IsDeleted = true;
        _ctx.TuitionPackages.Update(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
