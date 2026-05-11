using CLS.BLL.Common;
using CLS.BLL.DTOs.Sessions;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>
/// API quản lý buổi học — CLS-004 + CLS-005: Schedule Management.
/// </summary>
[ApiController]
[Route("api/v1/sessions")]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof(ApiResponse<object>), 401)]
[ProducesResponseType(typeof(ApiResponse<object>), 403)]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IActivityLogService _activityLogService;

    public SessionsController(ISessionService sessionService, IActivityLogService activityLogService)
    {
        _sessionService = sessionService;
        _activityLogService = activityLogService;
    }

    // ── POST /api/v1/sessions ─────────────────────────────────────────────────
    /// <summary>Tạo buổi học mới (auto-validate conflict).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SessionResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> CreateSession(
        [FromBody] CreateSessionRequest request, CancellationToken ct = default)
    {
        var result = await _sessionService.CreateSessionAsync(request, ct);
        if (result.IsSuccess)
            await this.LogActivityAsync(_activityLogService, AppConstants.ActionTypes.Create, $"Tạo buổi học lúc {request.StartTime:dd/MM/yyyy HH:mm}");
        return this.ToCreatedAtActionResponse(
            result,
            nameof(CreateSession),
            session => new { id = session.Id },
            "Tạo buổi học thành công.");
    }

    // ── PUT /api/v1/sessions/{id} ─────────────────────────────────────────────
    /// <summary>Cập nhật buổi học (auto-validate conflict).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SessionResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> UpdateSession(
        int id, [FromBody] UpdateSessionRequest request, CancellationToken ct = default)
    {
        var result = await _sessionService.UpdateSessionAsync(id, request, ct);
        if (result.IsSuccess)
            await this.LogActivityAsync(_activityLogService, AppConstants.ActionTypes.Update, $"Cập nhật buổi học #{id}");
        return this.ToOkResponse(result, "Cập nhật buổi học thành công.");
    }

    // ── DELETE /api/v1/sessions/{id} ──────────────────────────────────────────
    /// <summary>Xóa buổi học (soft-delete).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeleteSession(int id, CancellationToken ct = default)
    {
        var result = await _sessionService.DeleteSessionAsync(id, ct);
        if (result.IsSuccess)
            await this.LogActivityAsync(_activityLogService, AppConstants.ActionTypes.Delete, $"Xóa buổi học #{id}");
        return this.ToOkResponse(result, "Đã xóa buổi học thành công.");
    }

    // ── GET /api/v1/sessions?page&pageSize ────────────────────────────────────
    /// <summary>Lấy danh sách buổi học (phân trang).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SessionResponse>>), 200)]
    public async Task<IActionResult> GetAllSessions(
        [FromQuery] int page = AppConstants.Pagination.DefaultPage,
        [FromQuery] int pageSize = AppConstants.Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var result = await _sessionService.GetAllSessionsAsync(page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<SessionResponse>>.Success(result,
            "Lấy danh sách buổi học thành công."));
    }
}
