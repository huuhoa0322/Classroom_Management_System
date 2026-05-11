using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

/// <summary>
/// Repository cho activity_logs — READ + APPEND only.
/// Sắp xếp mặc định: mới nhất trước (OrderByDescending CreatedAt).
/// Include User để lấy tên người dùng.
/// </summary>
public class ActivityLogRepository : IActivityLogRepository
{
    private readonly AppDbContext _ctx;

    public ActivityLogRepository(AppDbContext ctx) => _ctx = ctx;

    /// <summary>Lấy danh sách log phân trang với bộ lọc tùy chọn.</summary>
    public async Task<(List<ActivityLog> Items, int Total)> GetPagedAsync(
        int page, int pageSize,
        int? userId = null,
        string? actionType = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default)
    {
        var query = _ctx.ActivityLogs
            .AsNoTracking()
            .Include(a => a.User)
            .AsQueryable();

        // ── Filters ──────────────────────────────────────────────────────────
        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(actionType))
            query = query.Where(a => a.ActionType == actionType);

        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.CreatedAt <= to.Value);

        // ── Sort: mới nhất trước ─────────────────────────────────────────────
        query = query.OrderByDescending(a => a.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    /// <summary>Thêm một log entry mới.</summary>
    public async Task AddAsync(ActivityLog log, CancellationToken ct = default)
        => await _ctx.ActivityLogs.AddAsync(log, ct);

    /// <summary>Lưu thay đổi vào database.</summary>
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
