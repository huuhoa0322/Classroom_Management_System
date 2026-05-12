using CLS.BLL.Common;
using CLS.BLL.DTOs;
using CLS.BLL.DTOs.Auth;
using CLS.BLL.Interfaces;
using CLS.DAL.Data;
using CLS.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _ctx;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    private readonly IActivityLogRepository _activityLogRepo;

    public AuthService(
        AppDbContext ctx, IJwtService jwtService, ILogger<AuthService> logger,
        IActivityLogRepository activityLogRepo)
    {
        _ctx             = ctx;
        _jwtService      = jwtService;
        _logger          = logger;
        _activityLogRepo = activityLogRepo;
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
        //    Bỏ AsNoTracking() — cần update lockout fields
        var user = await _ctx.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail
                                   && u.Status == AppConstants.UserAccountStatus.Active, ct);

        if (user is null)
        {
            _logger.LogWarning("Đăng nhập thất bại cho email {Email}", request.Email);
            return InvalidCredentials();
        }

        // 2a. Kiểm tra Admin manual lock
        if (user.IsLocked)
        {
            _logger.LogWarning("Account {Email} is manually locked by Admin", user.Email);
            return ServiceResult<LoginResponse>.Fail(
                "Tài khoản đã bị quản trị viên khóa. Vui lòng liên hệ Admin.",
                423,
                new { errorCode = "ACCOUNT_ADMIN_LOCKED" });
        }

        // 2b. Kiểm tra Account Lockout tự động (Security §M3)
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            var remaining = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes + 1;
            _logger.LogWarning(
                "Account locked for {Email}. Lockout ends at {LockoutEnd}",
                user.Email, user.LockoutEnd.Value);
            return AccountLocked(remaining);
        }

        // 3. Kiểm tra password với BCrypt
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            // Tăng failed count, lock nếu vượt ngưỡng
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= AppConstants.AccountLockout.MaxFailedAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(AppConstants.AccountLockout.LockoutMinutes);
                _logger.LogWarning(
                    "Account {Email} locked after {Count} failed attempts. Lockout until {LockoutEnd}",
                    user.Email, user.FailedLoginCount, user.LockoutEnd);
            }

            await _ctx.SaveChangesAsync(ct);

            _logger.LogWarning("Đăng nhập thất bại cho email {Email} (attempt {Count})",
                request.Email, user.FailedLoginCount);
            return InvalidCredentials();
        }

        // 4. Đăng nhập thành công — reset lockout state
        if (user.FailedLoginCount > 0 || user.LockoutEnd.HasValue)
        {
            user.FailedLoginCount = 0;
            user.LockoutEnd = null;
            await _ctx.SaveChangesAsync(ct);
        }

        // 5. Sinh JWT tokens
        var jwtUser = new JwtUserDto(
            Id:       user.Id,
            Email:    user.Email,
            Role:     user.Role,
            FullName: user.FullName
        );

        var accessToken  = _jwtService.GenerateAccessToken(jwtUser);
        var refreshToken = _jwtService.GenerateRefreshToken();

        _logger.LogInformation("User {Email} ({Role}) logged in successfully", user.Email, user.Role);

        // Ghi activity log — inline await, giữ đúng DI scope
        try
        {
            await _activityLogRepo.AddAsync(new DAL.Entities.ActivityLog
            {
                UserId = user.Id,
                ActionType = AppConstants.ActionTypes.Login,
                Description = $"Đăng nhập hệ thống ({user.Role})"
            });
            await _activityLogRepo.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write login activity log for User {UserId}", user.Id);
        }

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

    private static ServiceResult<LoginResponse> AccountLocked(int remainingMinutes)
        => ServiceResult<LoginResponse>.Fail(
            $"Tài khoản đã bị tạm khóa do nhập sai mật khẩu nhiều lần. Vui lòng thử lại sau {remainingMinutes} phút.",
            423,
            new { errorCode = "ACCOUNT_LOCKED", remainingMinutes });
}
