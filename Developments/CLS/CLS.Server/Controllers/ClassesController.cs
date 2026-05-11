using CLS.BLL.Common;
using CLS.BLL.DTOs.Classes;
using CLS.BLL.DTOs.Sessions;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CLS.Server.Controllers;

/// <summary>
/// API quản lý lớp học — CRUD + đăng ký học sinh.
/// </summary>
[ApiController]
[Route("api/v1/classes")]
[Authorize(Roles = "Admin")]
public class ClassesController : ControllerBase
{
    private readonly IClassService   _classService;
    private readonly ISessionService _sessionService;
    private readonly IActivityLogService _activityLogService;

    public ClassesController(IClassService classService, ISessionService sessionService, IActivityLogService activityLogService)
    {
        _classService   = classService;
        _sessionService = sessionService;
        _activityLogService = activityLogService;
    }

    // ── GET /api/v1/classes ───────────────────────────────────────────────────
    /// <summary>Lấy danh sách lớp (phân trang).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ClassResponse>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = AppConstants.Pagination.DefaultPage,
        [FromQuery] int pageSize = AppConstants.Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var result = await _classService.GetAllAsync(page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<ClassResponse>>.Success(result,
            "Lấy danh sách lớp thành công."));
    }

    // ── GET /api/v1/classes/dropdown ──────────────────────────────────────────
    /// <summary>Lấy danh sách lớp active (dropdown — flat list, không phân trang).</summary>
    [HttpGet("dropdown")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClassDto>>), 200)]
    public async Task<IActionResult> GetDropdown(CancellationToken ct = default)
    {
        var result = await _sessionService.GetClassesAsync(ct);
        return Ok(ApiResponse<IEnumerable<ClassDto>>.Success(result,
            "Lấy danh sách lớp thành công."));
    }

    // ── GET /api/v1/classes/{id} ──────────────────────────────────────────────
    /// <summary>Lấy chi tiết một lớp học.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ClassResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _classService.GetByIdAsync(id, ct);
        return this.ToOkResponse(result, "Lấy thông tin lớp học thành công.");
    }

    // ── POST /api/v1/classes ──────────────────────────────────────────────────
    /// <summary>Tạo lớp học mới.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ClassResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Create(
        [FromBody] CreateClassRequest request, CancellationToken ct = default)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiResponse<object>.Fail("Không xác định được người dùng.", 401));

        var result = await _classService.CreateAsync(request, userId, ct);
        if (result.IsSuccess)
            this.LogActivity(_activityLogService, AppConstants.ActionTypes.Create, $"Tạo lớp học: {request.Name}");
        return this.ToCreatedAtActionResponse(
            result,
            nameof(GetById),
            c => new { id = c.Id },
            "Tạo lớp học thành công.");
    }

    // ── PUT /api/v1/classes/{id} ──────────────────────────────────────────────
    /// <summary>Cập nhật tên lớp học.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ClassResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateClassRequest request, CancellationToken ct = default)
    {
        var result = await _classService.UpdateAsync(id, request, ct);
        if (result.IsSuccess)
            this.LogActivity(_activityLogService, AppConstants.ActionTypes.Update, $"Cập nhật lớp #{id}: {request.Name}");
        return this.ToOkResponse(result, "Cập nhật lớp học thành công.");
    }

    // ── PATCH /api/v1/classes/{id}/status ─────────────────────────────────────
    /// <summary>Đổi trạng thái lớp: active ↔ inactive.</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<ClassResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdateClassStatusRequest request, CancellationToken ct = default)
    {
        var result = await _classService.UpdateStatusAsync(id, request, ct);
        if (result.IsSuccess)
            this.LogActivity(_activityLogService, AppConstants.ActionTypes.StatusChange, $"Đổi trạng thái lớp #{id} → {request.Status}");
        return this.ToOkResponse(result, $"Cập nhật trạng thái lớp thành '{request.Status}' thành công.");
    }

    // ── GET /api/v1/classes/{id}/students ─────────────────────────────────────
    /// <summary>Lấy danh sách học sinh trong lớp.</summary>
    [HttpGet("{id:int}/students")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClassStudentResponse>>), 200)]
    public async Task<IActionResult> GetStudents(int id, CancellationToken ct = default)
    {
        var result = await _classService.GetStudentsAsync(id, ct);
        return Ok(ApiResponse<IEnumerable<ClassStudentResponse>>.Success(result,
            "Lấy danh sách học sinh trong lớp thành công."));
    }

    // ── POST /api/v1/classes/{id}/students ────────────────────────────────────
    /// <summary>Đăng ký nhiều học sinh vào lớp cùng lúc.</summary>
    [HttpPost("{id:int}/students")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClassStudentResponse>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> EnrollStudents(
        int id, [FromBody] EnrollStudentsRequest request, CancellationToken ct = default)
    {
        var result = await _classService.EnrollStudentsAsync(id, request, ct);
        return this.ToOkResponse(result, "Đăng ký học sinh vào lớp thành công.");
    }

    // ── DELETE /api/v1/classes/{id}/students/{studentId} ──────────────────────
    /// <summary>Hủy đăng ký học sinh khỏi lớp.</summary>
    [HttpDelete("{id:int}/students/{studentId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UnenrollStudent(
        int id, int studentId, CancellationToken ct = default)
    {
        var result = await _classService.UnenrollStudentAsync(id, studentId, ct);
        return this.ToOkResponse(result, "Hủy đăng ký học sinh khỏi lớp thành công.");
    }
}
