using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

public class TuitionPackageRepository : ITuitionPackageRepository
{
    private readonly AppDbContext _ctx;

    public TuitionPackageRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<TuitionPackage?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.TuitionPackages.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IEnumerable<TuitionPackage>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.TuitionPackages.AsNoTracking().ToListAsync(ct);

    public async Task<List<TuitionPackage>> GetAllActiveAsync(CancellationToken ct = default)
        => await _ctx.TuitionPackages
            .AsNoTracking()
            .Where(p => p.Status == Common.DalConstants.Status.Active)
            .OrderBy(p => p.TotalSessions)
            .ToListAsync(ct);

    public async Task<(List<TuitionPackage> Items, int Total)> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _ctx.TuitionPackages
            .AsNoTracking()
            .Include(p => p.StudentPackages)
            .OrderBy(p => p.Name);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        var normalizedName = name.Trim().ToLower();
        var query = _ctx.TuitionPackages.Where(p => p.Name.ToLower() == normalizedName);
        if (excludeId.HasValue) query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task AddAsync(TuitionPackage entity, CancellationToken ct = default)
        => await _ctx.TuitionPackages.AddAsync(entity, ct);

    public void Update(TuitionPackage entity) => _ctx.TuitionPackages.Update(entity);

    public void Delete(TuitionPackage entity)
    {
        entity.IsDeleted = true;
        _ctx.TuitionPackages.Update(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
