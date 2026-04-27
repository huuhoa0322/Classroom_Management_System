using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly AppDbContext _ctx;

    public AttendanceRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Attendance?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.Attendances.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IEnumerable<Attendance>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Attendances.AsNoTracking().ToListAsync(ct);

    public async Task<List<Attendance>> GetBySessionIdAsync(int sessionId, CancellationToken ct = default)
        => await _ctx.Attendances
            .AsNoTracking()
            .Include(a => a.Student)
            .Where(a => a.SessionId == sessionId)
            .OrderBy(a => a.Student.FullName)
            .ToListAsync(ct);

    public async Task<bool> HasAttendanceAsync(int sessionId, CancellationToken ct = default)
        => await _ctx.Attendances.AnyAsync(a => a.SessionId == sessionId, ct);

    public async Task AddRangeAsync(IEnumerable<Attendance> records, CancellationToken ct = default)
        => await _ctx.Attendances.AddRangeAsync(records, ct);

    public async Task AddAsync(Attendance entity, CancellationToken ct = default)
        => await _ctx.Attendances.AddAsync(entity, ct);

    public void Update(Attendance entity)
        => _ctx.Attendances.Update(entity);

    public void Delete(Attendance entity)
    {
        entity.IsDeleted = true;
        _ctx.Attendances.Update(entity);
    }

    public async Task DeleteBySessionIdAsync(int sessionId, CancellationToken ct = default)
    {
        var records = await _ctx.Attendances
            .Where(a => a.SessionId == sessionId)
            .ToListAsync(ct);
        _ctx.Attendances.RemoveRange(records);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
