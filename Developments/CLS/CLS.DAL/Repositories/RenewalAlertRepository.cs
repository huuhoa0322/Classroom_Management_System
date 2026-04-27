using CLS.DAL.Data;
using CLS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLS.DAL.Repositories;

public class RenewalAlertRepository : IRenewalAlertRepository
{
    private readonly AppDbContext _ctx;

    public RenewalAlertRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<List<AlertNotification>> GetAlertsPagedAsync(
        int page, int pageSize,
        string? statusFilter,
        string? sortBy, string? sortDir,
        CancellationToken ct = default)
    {
        var query = _ctx.AlertNotifications
            .AsNoTracking()
            .Where(a => a.Type == "renewal_alert" || a.Type == "package_depletion")
            .Include(a => a.StudentPackage!)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.Parent)
            .Include(a => a.StudentPackage!)
                .ThenInclude(sp => sp.Package)
            .AsQueryable();

        // ── Status filter ────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(statusFilter))
            query = query.Where(a => a.Status == statusFilter);

        // ── Sorting ──────────────────────────────────────────────────────────
        var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = sortBy?.ToLowerInvariant() switch
        {
            "studentname"       => isDesc
                ? query.OrderByDescending(a => a.StudentPackage!.Student.FullName)
                : query.OrderBy(a => a.StudentPackage!.Student.FullName),
            "remainingsessions" => isDesc
                ? query.OrderByDescending(a => a.StudentPackage!.RemainingSessions)
                : query.OrderBy(a => a.StudentPackage!.RemainingSessions),
            "remainingdays"     => isDesc
                ? query.OrderByDescending(a => a.StudentPackage!.EndDate)
                : query.OrderBy(a => a.StudentPackage!.EndDate),
            "createdat"         => isDesc
                ? query.OrderByDescending(a => a.CreatedAt)
                : query.OrderBy(a => a.CreatedAt),
            _                   => query.OrderByDescending(a => a.CreatedAt)
        };

        // ── Pagination ───────────────────────────────────────────────────────
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(string? statusFilter, CancellationToken ct = default)
    {
        var query = _ctx.AlertNotifications
            .Where(a => a.Type == "renewal_alert" || a.Type == "package_depletion")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter))
            query = query.Where(a => a.Status == statusFilter);

        return await query.CountAsync(ct);
    }

    public async Task<AlertNotification?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.AlertNotifications
            .Include(a => a.StudentPackage!)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.Parent)
            .Include(a => a.StudentPackage!)
                .ThenInclude(sp => sp.Package)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<bool> ExistsForPackageAsync(int studentPackageId, string status, CancellationToken ct = default)
        => await _ctx.AlertNotifications
            .AnyAsync(a => a.StudentPackageId == studentPackageId
                        && a.Status == status, ct);

    public async Task<HashSet<int>> GetExistingAlertPackageIdsAsync(CancellationToken ct = default)
    {
        var ids = await _ctx.AlertNotifications
            .Where(a => a.StudentPackageId != null)
            .Select(a => a.StudentPackageId!.Value)
            .Distinct()
            .ToListAsync(ct);

        return ids.ToHashSet();
    }

    public async Task<List<AlertNotification>> GetUnsentForDispatchAsync(int batchSize, CancellationToken ct = default)
        => await _ctx.AlertNotifications
            .Where(a => a.EmailSentAt == null)
            .Include(a => a.StudentPackage!)
                .ThenInclude(sp => sp.Student)
                    .ThenInclude(s => s.Parent)
            .Include(a => a.StudentPackage!)
                .ThenInclude(sp => sp.Package)
            .OrderBy(a => a.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);

    public async Task AddAsync(AlertNotification entity, CancellationToken ct = default)
        => await _ctx.AlertNotifications.AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<AlertNotification> entities, CancellationToken ct = default)
        => await _ctx.AlertNotifications.AddRangeAsync(entities, ct);

    public void Update(AlertNotification entity)
        => _ctx.AlertNotifications.Update(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
