using CLS.BLL.Common;
using CLS.BLL.DTOs.Sessions;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>
/// API danh sách giáo viên — dùng cho dropdown trong Schedule Maker.
/// </summary>
[ApiController]
[Route("api/v1/teachers")]
[Authorize(Roles = "Admin")]
public class TeachersController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public TeachersController(ISessionService sessionService)
        => _sessionService = sessionService;

    /// <summary>Lấy danh sách giáo viên active (dropdown).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TeacherDto>>), 200)]
    public async Task<IActionResult> GetTeachers(CancellationToken ct = default)
    {
        var result = await _sessionService.GetTeachersAsync(ct);
        return Ok(ApiResponse<IEnumerable<TeacherDto>>.Success(result,
            "Lấy danh sách giáo viên thành công."));
    }
}
