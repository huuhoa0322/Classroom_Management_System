using CLS.BLL.Common;
using CLS.BLL.DTOs;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>API Dashboard — thống kê tổng quan cho Admin.</summary>
[ApiController]
[Route("api/v1/dashboard")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    /// <summary>Lấy thống kê Dashboard.</summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatsResponse>), 200)]
    public async Task<IActionResult> GetStats(CancellationToken ct = default)
    {
        var stats = await _dashboardService.GetStatsAsync(ct);
        return Ok(ApiResponse<DashboardStatsResponse>.Success(stats, "Lấy thống kê thành công."));
    }
}
