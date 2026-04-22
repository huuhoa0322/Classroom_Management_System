using CLS.BLL.Common;
using CLS.BLL.Common.Exceptions;
using CLS.BLL.DTOs;
using CLS.BLL.DTOs.Auth;
using CLS.BLL.Interfaces;
using CLS.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _ctx;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext ctx, IJwtService jwtService, ILogger<AuthService> logger)
    {
        _ctx        = ctx;
        _jwtService = jwtService;
        _logger     = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        // 1. Tìm user theo email (case-insensitive, chỉ active)
        var user = await _ctx.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower()
                                   && u.Status == "active", ct);

        if (user is null)
            throw new UnauthorizedException("Email hoặc mật khẩu không chính xác.");

        // 2. Kiểm tra password với BCrypt
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Failed login attempt for email {Email}", request.Email);
            throw new UnauthorizedException("Email hoặc mật khẩu không chính xác.");
        }

        // 3. Sinh JWT tokens
        var jwtUser = new JwtUserDto(
            Id:       user.Id,
            Email:    user.Email,
            Role:     user.Role,
            FullName: user.FullName
        );

        var accessToken  = _jwtService.GenerateAccessToken(jwtUser);
        var refreshToken = _jwtService.GenerateRefreshToken();

        _logger.LogInformation("User {Email} ({Role}) logged in successfully", user.Email, user.Role);

        return new LoginResponse
        {
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            User = new UserInfo
            {
                Id       = user.Id,
                FullName = user.FullName,
                Email    = user.Email,
                Role     = user.Role,
            }
        };
    }
}
