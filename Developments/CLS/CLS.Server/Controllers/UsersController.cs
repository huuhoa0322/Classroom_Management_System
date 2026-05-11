using CLS.BLL.Common;
using CLS.BLL.DTOs.Users;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>
/// API quản lý tài khoản — CRUD Teacher + reset password.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userService;
    private readonly IActivityLogService _activityLogService;

    public UsersController(IUserManagementService userService, IActivityLogService activityLogService)
    {
        _userService = userService;
        _activityLogService = activityLogService;
    }

    /// <summary>Lấy danh sách tài khoản (phân trang).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserResponse>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = AppConstants.Pagination.DefaultPage,
        [FromQuery] int pageSize = AppConstants.Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var result = await _userService.GetAllAsync(page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<UserResponse>>.Success(result, "Lấy danh sách tài khoản thành công."));
    }

    /// <summary>Lấy chi tiết tài khoản.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        return this.ToOkResponse(result, "Lấy thông tin tài khoản thành công.");
    }

    /// <summary>Tạo tài khoản Teacher mới.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct = default)
    {
        var result = await _userService.CreateTeacherAsync(request, ct);
        if (result.IsSuccess)
            await this.LogActivityAsync(_activityLogService, AppConstants.ActionTypes.Create, $"Tạo tài khoản giáo viên: {request.Email}");
        return this.ToCreatedAtActionResponse(result, nameof(GetById), u => new { id = u.Id }, "Tạo tài khoản giáo viên thành công.");
    }

    /// <summary>Cập nhật thông tin tài khoản.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken ct = default)
    {
        var result = await _userService.UpdateAsync(id, request, ct);
        if (result.IsSuccess)
            await this.LogActivityAsync(_activityLogService, AppConstants.ActionTypes.Update, $"Cập nhật tài khoản #{id}: {request.Email}");
        return this.ToOkResponse(result, "Cập nhật tài khoản thành công.");
    }

    /// <summary>Đổi trạng thái tài khoản: active ↔ inactive.</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateUserStatusRequest request, CancellationToken ct = default)
    {
        var result = await _userService.UpdateStatusAsync(id, request, ct);
        if (result.IsSuccess)
            await this.LogActivityAsync(_activityLogService, AppConstants.ActionTypes.StatusChange, $"Đổi trạng thái tài khoản #{id} → {request.Status}");
        return this.ToOkResponse(result, "Cập nhật trạng thái tài khoản thành công.");
    }

    /// <summary>Đặt lại mật khẩu ngẫu nhiên cho tài khoản.</summary>
    [HttpPatch("{id:int}/reset-password")]
    [ProducesResponseType(typeof(ApiResponse<ResetPasswordResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> ResetPassword(int id, CancellationToken ct = default)
    {
        var result = await _userService.ResetPasswordAsync(id, ct);
        if (result.IsSuccess)
            await this.LogActivityAsync(_activityLogService, AppConstants.ActionTypes.Update, $"Reset mật khẩu tài khoản #{id}");
        return this.ToOkResponse(result, "Đặt lại mật khẩu thành công.");
    }
}
