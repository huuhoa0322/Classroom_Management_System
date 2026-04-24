using CLS.BLL.Common;
using CLS.BLL.DTOs.Sessions;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>
/// API danh mục lớp học — dùng cho dropdown trong Schedule Maker.
/// </summary>
[ApiController]
[Route("api/v1/classes")]
[Authorize(Roles = "Admin")]
public class ClassesController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public ClassesController(ISessionService sessionService)
        => _sessionService = sessionService;

    /// <summary>Lấy danh sách lớp active (dropdown).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClassDto>>), 200)]
    public async Task<IActionResult> GetClasses(CancellationToken ct = default)
    {
        var result = await _sessionService.GetClassesAsync(ct);
        return Ok(ApiResponse<IEnumerable<ClassDto>>.Success(result,
            "Lấy danh sách lớp thành công."));
    }
}
