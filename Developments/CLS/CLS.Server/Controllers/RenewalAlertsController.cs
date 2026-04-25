using CLS.BLL.Common;
using CLS.BLL.DTOs.RenewalAlerts;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>
/// API cảnh báo gia hạn gói học — CLS-006: Review Package Renewal Alerts.
/// </summary>
[ApiController]
[Route(AppConstants.Routes.Alerts)]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof(ApiResponse<object>), 401)]
[ProducesResponseType(typeof(ApiResponse<object>), 403)]
public class RenewalAlertsController : ControllerBase
{
    private readonly IRenewalAlertService _alertService;

    public RenewalAlertsController(IRenewalAlertService alertService)
        => _alertService = alertService;

    // ── GET /api/v1/renewal-alerts ────────────────────────────────────────────
    /// <summary>Lấy danh sách cảnh báo gia hạn (phân trang, sort, filter).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RenewalAlertResponse>>), 200)]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] int page = AppConstants.Pagination.DefaultPage,
        [FromQuery] int pageSize = AppConstants.Pagination.DefaultPageSize,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken ct = default)
    {
        var result = await _alertService.GetAlertsAsync(page, pageSize, status, sortBy, sortDir, ct);
        return Ok(ApiResponse<PagedResult<RenewalAlertResponse>>.Success(
            result, "Lấy danh sách cảnh báo gia hạn thành công."));
    }

    // ── PATCH /api/v1/renewal-alerts/{id}/status ──────────────────────────────
    /// <summary>Cập nhật trạng thái cảnh báo (pending ↔ consulted).</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<RenewalAlertResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateAlertStatus(
        int id, [FromBody] UpdateAlertStatusRequest request, CancellationToken ct = default)
    {
        var result = await _alertService.UpdateAlertStatusAsync(id, request, ct);
        return this.ToOkResponse(result, $"Cập nhật trạng thái cảnh báo thành '{request.Status}' thành công.");
    }
}
