using CLS.BLL.Common;
using CLS.BLL.DTOs.ActivityLogs;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>
/// API nhật ký hoạt động hệ thống — READ-ONLY cho Admin.
/// </summary>
[ApiController]
[Route("api/v1/activity-logs")]
[Authorize(Roles = "Admin")]
public class ActivityLogsController : ControllerBase
{
    private readonly IActivityLogService _activityLogService;

    public ActivityLogsController(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    /// <summary>Lấy danh sách nhật ký hoạt động (phân trang + bộ lọc).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ActivityLogResponse>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = AppConstants.Pagination.DefaultPage,
        [FromQuery] int pageSize = AppConstants.Pagination.DefaultPageSize,
        [FromQuery] int? userId = null,
        [FromQuery] string? actionType = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        // Clamp pageSize — bảng append-only tăng nhanh, cần chặn abuse
        pageSize = Math.Clamp(pageSize, 1, AppConstants.Pagination.MaxPageSize);

        var result = await _activityLogService.GetAllAsync(page, pageSize, userId, actionType, from, to, ct);
        return Ok(ApiResponse<PagedResult<ActivityLogResponse>>.Success(result, "Lấy nhật ký hoạt động thành công."));
    }
}
