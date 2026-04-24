using CLS.BLL.Common;
using CLS.BLL.DTOs.Sessions;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>
/// API danh mục phòng học — dùng cho dropdown trong Schedule Maker.
/// </summary>
[ApiController]
[Route("api/v1/rooms")]
[Authorize(Roles = "Admin")]
public class RoomsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public RoomsController(ISessionService sessionService)
        => _sessionService = sessionService;

    /// <summary>Lấy danh sách phòng active (dropdown).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoomDto>>), 200)]
    public async Task<IActionResult> GetRooms(CancellationToken ct = default)
    {
        var result = await _sessionService.GetRoomsAsync(ct);
        return Ok(ApiResponse<IEnumerable<RoomDto>>.Success(result,
            "Lấy danh sách phòng thành công."));
    }
}
