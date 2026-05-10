using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _ctx;

    public RoomRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Room?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.Rooms.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IEnumerable<Room>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Rooms.AsNoTracking().ToListAsync(ct);

    public async Task<List<Room>> GetAllActiveAsync(CancellationToken ct = default)
        => await _ctx.Rooms
            .AsNoTracking()
            .Where(r => r.Status == Common.DalConstants.Status.Active)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

    public async Task<(List<Room> Items, int Total)> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _ctx.Rooms.AsNoTracking().OrderBy(r => r.Name);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        var normalizedName = name.Trim().ToLower();
        var query = _ctx.Rooms.Where(r => r.Name.ToLower() == normalizedName);
        if (excludeId.HasValue) query = query.Where(r => r.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task AddAsync(Room entity, CancellationToken ct = default)
        => await _ctx.Rooms.AddAsync(entity, ct);

    public void Update(Room entity) => _ctx.Rooms.Update(entity);

    public void Delete(Room entity)
    {
        entity.IsDeleted = true;
        _ctx.Rooms.Update(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
