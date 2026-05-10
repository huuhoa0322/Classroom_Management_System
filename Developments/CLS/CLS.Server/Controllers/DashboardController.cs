using CLS.BLL.Common;
using CLS.BLL.DTOs;
using CLS.DAL.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLS.Server.Controllers;

/// <summary>API Dashboard — thống kê tổng quan cho Admin.</summary>
[ApiController]
[Route("api/v1/dashboard")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public DashboardController(AppDbContext ctx) => _ctx = ctx;

    /// <summary>Lấy thống kê Dashboard.</summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatsResponse>), 200)]
    public async Task<IActionResult> GetStats(CancellationToken ct = default)
    {
        var stats = new DashboardStatsResponse
        {
            TotalStudents = await _ctx.Students.CountAsync(s => s.Status == AppConstants.StudentStatus.Active, ct),
            TotalClasses = await _ctx.Classes.CountAsync(c => c.Status == AppConstants.ClassStatus.Active, ct),
            TotalTeachers = await _ctx.Users.CountAsync(u => u.Role == AppConstants.AppRoles.Teacher && u.Status == AppConstants.UserAccountStatus.Active, ct),
            PendingAlerts = await _ctx.AlertNotifications.CountAsync(a => a.Status == AppConstants.AlertNotificationStatus.Pending, ct),
            TotalRevenue = await _ctx.Payments.Where(p => p.Status == AppConstants.PaymentStatus.Confirmed).SumAsync(p => p.Amount, ct),
            UpcomingSessions = await _ctx.Sessions.CountAsync(s => s.Status == AppConstants.SessionStatus.Scheduled && s.StartTime >= DateTime.UtcNow, ct),
        };

        return Ok(ApiResponse<DashboardStatsResponse>.Success(stats, "Lấy thống kê thành công."));
    }
}
