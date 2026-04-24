using CLS.BLL.Common;
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

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ServiceResult<LoginResponse>.Fail(
                "Email và mật khẩu là bắt buộc.",
                400,
                new
                {
                    errors = new Dictionary<string, string[]>
                    {
                        ["email"] = string.IsNullOrWhiteSpace(request.Email)
                            ? ["Email là bắt buộc."]
                            : [],
                        ["password"] = string.IsNullOrWhiteSpace(request.Password)
                            ? ["Mật khẩu là bắt buộc."]
                            : []
                    }
                    .Where(pair => pair.Value.Length > 0)
                    .ToDictionary(pair => pair.Key, pair => pair.Value)
                });
        }

        var normalizedEmail = request.Email.Trim().ToLower();

        // 1. Tìm user theo email (case-insensitive, chỉ active)
        var user = await _ctx.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail
                                   && u.Status == "active", ct);

        if (user is null)
        {
            _logger.LogWarning("Đăng nhập thất bại cho email {Email}", request.Email);
            return InvalidCredentials();
        }

        // 2. Kiểm tra password với BCrypt
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Đăng nhập thất bại cho email {Email}", request.Email);
            return InvalidCredentials();
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

        return ServiceResult<LoginResponse>.Success(
            new LoginResponse
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
            });
    }

    private static ServiceResult<LoginResponse> InvalidCredentials()
        => ServiceResult<LoginResponse>.Fail(
            "Email hoặc mật khẩu không chính xác.",
            401,
            new { errorCode = "INVALID_CREDENTIALS" });
}
