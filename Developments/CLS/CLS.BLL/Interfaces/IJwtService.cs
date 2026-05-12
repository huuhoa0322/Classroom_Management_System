using System.Security.Claims;
using CLS.BLL.DTOs;

namespace CLS.BLL.Interfaces;

/// <summary>
/// Contract cho JWT token operations trong CLS.
/// Implement bởi JwtService trong CLS.BLL/Services/.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Tạo JWT Access Token từ thông tin user.
    /// Token chứa claims: sub (Id), email, role, name, jti.
    /// Expiry đọc từ appsettings.json → JwtSettings:AccessTokenExpiryMinutes (default 60 phút).
    /// </summary>
    string GenerateAccessToken(JwtUserDto user);

    /// <summary>
    /// Tạo Refresh Token ngẫu nhiên an toàn (cryptographically secure).
    /// Cần lưu hash vào DB để validate sau.
    /// Expiry đọc từ appsettings.json → JwtSettings:RefreshTokenExpiryDays (default 7 ngày).
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate JWT token và trả về ClaimsPrincipal nếu hợp lệ.
    /// Validate: signature, issuer, audience. KHÔNG validate lifetime
    /// (dùng cho refresh flow — access token đã hết hạn nhưng cần đọc claims).
    /// Trả về null nếu token invalid hoặc có exception.
    /// </summary>
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
}
