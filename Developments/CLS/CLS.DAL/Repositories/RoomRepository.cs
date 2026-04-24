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
            .Where(r => r.Status == "active")
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

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
