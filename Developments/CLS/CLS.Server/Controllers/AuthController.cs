using CLS.BLL.Common;
using CLS.BLL.DTOs.Auth;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>
/// API xác thực người dùng nội bộ (Admin, Teacher).
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Đăng nhập bằng Email + Password, nhận JWT access token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct = default)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Ok(ApiResponse<LoginResponse>.Success(result, "Đăng nhập thành công."));
    }
}
