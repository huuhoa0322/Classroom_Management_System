using CLS.BLL.Common;
using CLS.BLL.DTOs.Attendance;
using CLS.BLL.DTOs.Sessions;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>
/// API dành cho Teacher — UC-07: Lịch dạy, UC-08: Điểm danh.
/// </summary>
[ApiController]
[Route("api/v1/teacher")]
[Authorize(Roles = "Teacher")]
[ProducesResponseType(typeof(ApiResponse<object>), 401)]
[ProducesResponseType(typeof(ApiResponse<object>), 403)]
public class TeacherController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public TeacherController(IAttendanceService attendanceService)
        => _attendanceService = attendanceService;

    /// <summary>
    /// Lấy Teacher ID từ JWT Claims (sub).
    /// </summary>
    private int GetTeacherId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var id) ? id : 0;
    }

    // ── GET /api/v1/teacher/timetable?from=&to= ───────────────────────────────
    /// <summary>UC-07: Lấy lịch dạy trong khoảng thời gian (mặc định: tuần hiện tại).</summary>
    [HttpGet("timetable")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionResponse>>), 200)]
    public async Task<IActionResult> GetTimetable(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var teacherId = GetTeacherId();
        if (teacherId == 0)
            return Unauthorized(ApiResponse<object>.Fail("Không xác định được Teacher từ token.", 401));

        // Default: tuần hiện tại (Monday → Sunday)
        var today    = DateTime.UtcNow.Date;
        var monday   = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        var fromDate = from ?? monday;
        var toDate   = to ?? monday.AddDays(7);

        var sessions = await _attendanceService.GetTimetableAsync(teacherId, fromDate, toDate, ct);
        return Ok(ApiResponse<IEnumerable<SessionResponse>>.Success(sessions,
            "Lấy lịch dạy thành công."));
    }

    // ── GET /api/v1/teacher/sessions/{id}/attendance ──────────────────────────
    /// <summary>UC-08: Lấy sheet điểm danh (session info + danh sách học sinh).</summary>
    [HttpGet("sessions/{id:int}/attendance")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceSheetDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetAttendanceSheet(int id, CancellationToken ct = default)
    {
        var teacherId = GetTeacherId();
        if (teacherId == 0)
            return Unauthorized(ApiResponse<object>.Fail("Không xác định được Teacher từ token.", 401));

        var result = await _attendanceService.GetAttendanceSheetAsync(id, teacherId, ct);
        return this.ToOkResponse(result, "Lấy sheet điểm danh thành công.");
    }

    // ── POST /api/v1/teacher/sessions/{id}/attendance ────────────────────────
    /// <summary>UC-08: Submit điểm danh cho buổi học.</summary>
    [HttpPost("sessions/{id:int}/attendance")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> SubmitAttendance(
        int id,
        [FromBody] SubmitAttendanceRequest request,
        CancellationToken ct = default)
    {
        var teacherId = GetTeacherId();
        if (teacherId == 0)
            return Unauthorized(ApiResponse<object>.Fail("Không xác định được Teacher từ token.", 401));

        var result = await _attendanceService.SubmitAttendanceAsync(id, teacherId, request, ct);
        return this.ToOkResponse(result, "Điểm danh thành công.");
    }
}
