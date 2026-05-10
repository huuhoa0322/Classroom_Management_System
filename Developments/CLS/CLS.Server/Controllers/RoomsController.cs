using CLS.BLL.Common;
using CLS.BLL.DTOs.Rooms;
using CLS.BLL.DTOs.Sessions;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>
/// API quản lý phòng học — CRUD + dropdown.
/// </summary>
[ApiController]
[Route("api/v1/rooms")]
[Authorize(Roles = "Admin")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ISessionService _sessionService;

    public RoomsController(IRoomService roomService, ISessionService sessionService)
    {
        _roomService = roomService;
        _sessionService = sessionService;
    }

    /// <summary>Lấy danh sách phòng (phân trang).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RoomResponse>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = AppConstants.Pagination.DefaultPage,
        [FromQuery] int pageSize = AppConstants.Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var result = await _roomService.GetAllAsync(page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<RoomResponse>>.Success(result, "Lấy danh sách phòng thành công."));
    }

    /// <summary>Lấy danh sách phòng active (dropdown).</summary>
    [HttpGet("dropdown")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoomDto>>), 200)]
    public async Task<IActionResult> GetDropdown(CancellationToken ct = default)
    {
        var result = await _sessionService.GetRoomsAsync(ct);
        return Ok(ApiResponse<IEnumerable<RoomDto>>.Success(result, "Lấy danh sách phòng thành công."));
    }

    /// <summary>Lấy chi tiết phòng.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RoomResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _roomService.GetByIdAsync(id, ct);
        return this.ToOkResponse(result, "Lấy thông tin phòng thành công.");
    }

    /// <summary>Tạo phòng mới.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RoomResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest request, CancellationToken ct = default)
    {
        var result = await _roomService.CreateAsync(request, ct);
        return this.ToCreatedAtActionResponse(result, nameof(GetById), r => new { id = r.Id }, "Tạo phòng thành công.");
    }

    /// <summary>Cập nhật phòng.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RoomResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomRequest request, CancellationToken ct = default)
    {
        var result = await _roomService.UpdateAsync(id, request, ct);
        return this.ToOkResponse(result, "Cập nhật phòng thành công.");
    }

    /// <summary>Đổi trạng thái phòng: active ↔ inactive.</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<RoomResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateRoomStatusRequest request, CancellationToken ct = default)
    {
        var result = await _roomService.UpdateStatusAsync(id, request, ct);
        return this.ToOkResponse(result, $"Cập nhật trạng thái phòng thành '{request.Status}' thành công.");
    }
}
