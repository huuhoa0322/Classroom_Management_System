using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _ctx;

    public SessionRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Session?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.Sessions.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IEnumerable<Session>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Sessions.AsNoTracking().ToListAsync(ct);

    public async Task<(List<Session> Items, int TotalCount)> GetPagedAllAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _ctx.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .Include(s => s.Teacher)
            .Include(s => s.Room);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Session?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await _ctx.Sessions
            .Include(s => s.Class)
            .Include(s => s.Teacher)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    // ── CLS-005 AC1: Conflict Detection — Teacher ────────────────────────────
    public async Task<bool> HasTeacherConflictAsync(
        int teacherId, DateTime startTime, DateTime endTime,
        int? excludeSessionId = null, CancellationToken ct = default)
    {
        var query = _ctx.Sessions
            .Where(s => s.TeacherId == teacherId
                     && s.Status != "cancelled"
                     && s.StartTime < endTime
                     && s.EndTime > startTime);

        if (excludeSessionId.HasValue)
            query = query.Where(s => s.Id != excludeSessionId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<Session?> GetTeacherConflictAsync(
        int teacherId, DateTime startTime, DateTime endTime,
        int? excludeSessionId = null, CancellationToken ct = default)
    {
        var query = _ctx.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .Include(s => s.Teacher)
            .Include(s => s.Room)
            .Where(s => s.TeacherId == teacherId
                     && s.Status != "cancelled"
                     && s.StartTime < endTime
                     && s.EndTime > startTime);

        if (excludeSessionId.HasValue)
            query = query.Where(s => s.Id != excludeSessionId.Value);

        return await query.OrderBy(s => s.StartTime).FirstOrDefaultAsync(ct);
    }

    // ── CLS-005 AC2: Conflict Detection — Room ──────────────────────────────
    public async Task<bool> HasRoomConflictAsync(
        int roomId, DateTime startTime, DateTime endTime,
        int? excludeSessionId = null, CancellationToken ct = default)
    {
        var query = _ctx.Sessions
            .Where(s => s.RoomId == roomId
                     && s.Status != "cancelled"
                     && s.StartTime < endTime
                     && s.EndTime > startTime);

        if (excludeSessionId.HasValue)
            query = query.Where(s => s.Id != excludeSessionId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<Session?> GetRoomConflictAsync(
        int roomId, DateTime startTime, DateTime endTime,
        int? excludeSessionId = null, CancellationToken ct = default)
    {
        var query = _ctx.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .Include(s => s.Teacher)
            .Include(s => s.Room)
            .Where(s => s.RoomId == roomId
                     && s.Status != "cancelled"
                     && s.StartTime < endTime
                     && s.EndTime > startTime);

        if (excludeSessionId.HasValue)
            query = query.Where(s => s.Id != excludeSessionId.Value);

        return await query.OrderBy(s => s.StartTime).FirstOrDefaultAsync(ct);
    }

    // ── UC-07: Teacher Timetable ──────────────────────────────────────────────
    public async Task<List<Session>> GetTeacherScheduleAsync(
        int teacherId, DateTime from, DateTime to, CancellationToken ct = default)
        => await _ctx.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .Include(s => s.Room)
            .Where(s => s.TeacherId == teacherId
                     && s.StartTime >= from
                     && s.StartTime < to
                     && s.Status != "cancelled") // TODO: consider DAL constants
            .OrderBy(s => s.StartTime)
            .ToListAsync(ct);

    // ── UC-08: Attendance Sheet — Session + Class Students ────────────────────
    public async Task<Session?> GetByIdWithClassStudentsAsync(
        int sessionId, CancellationToken ct = default)
        => await _ctx.Sessions
            .Include(s => s.Class)
                .ThenInclude(c => c.ClassStudents.Where(cs => cs.Status == "active" && !cs.IsDeleted)) // TODO: consider DAL constants
                    .ThenInclude(cs => cs.Student)
            .Include(s => s.Room)
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

    public async Task AddAsync(Session entity, CancellationToken ct = default)
        => await _ctx.Sessions.AddAsync(entity, ct);

    public void Update(Session entity)
        => _ctx.Sessions.Update(entity);

    public void Delete(Session entity)
    {
        entity.IsDeleted = true;
        _ctx.Sessions.Update(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
