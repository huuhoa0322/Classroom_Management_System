using CLS.BLL.Common;
using CLS.BLL.DTOs;
using CLS.BLL.Interfaces;
using CLS.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CLS.BLL.Services;

/// <summary>
/// Service tổng hợp thống kê Dashboard — read-only cross-module reporting.
/// Sử dụng AppDbContext trực tiếp vì đây là truy vấn aggregate tổng hợp,
/// không thuộc trách nhiệm của bất kỳ module repository đơn lẻ nào.
///
/// Lỗi kết nối DB (NpgsqlException, SocketException, ...) sẽ propagate lên
/// ApiExceptionFilter và được tự động phát hiện → trả về 503 Service Unavailable.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _ctx;

    public DashboardService(AppDbContext ctx) => _ctx = ctx;

    /// <inheritdoc />
    public async Task<DashboardStatsResponse> GetStatsAsync(CancellationToken ct = default)
    {
        return new DashboardStatsResponse
        {
            TotalStudents = await _ctx.Students
                .AsNoTracking()
                .CountAsync(s => s.Status == AppConstants.StudentStatus.Active, ct),

            TotalClasses = await _ctx.Classes
                .AsNoTracking()
                .CountAsync(c => c.Status == AppConstants.ClassStatus.Active, ct),

            TotalTeachers = await _ctx.Users
                .AsNoTracking()
                .CountAsync(u => u.Role == AppConstants.AppRoles.Teacher
                              && u.Status == AppConstants.UserAccountStatus.Active, ct),

            PendingAlerts = await _ctx.AlertNotifications
                .AsNoTracking()
                .CountAsync(a => a.Status == AppConstants.AlertNotificationStatus.Pending, ct),

            TotalRevenue = await _ctx.Payments
                .AsNoTracking()
                .Where(p => p.Status == AppConstants.PaymentStatus.Confirmed)
                .SumAsync(p => p.Amount, ct),

            UpcomingSessions = await _ctx.Sessions
                .AsNoTracking()
                .CountAsync(s => s.Status == AppConstants.SessionStatus.Scheduled
                              && s.StartTime >= DateTime.UtcNow, ct),
        };
    }
}
