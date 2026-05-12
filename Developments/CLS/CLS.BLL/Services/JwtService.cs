using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CLS.BLL.DTOs;
using CLS.BLL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CLS.BLL.Services;

/// <summary>
/// Implementation của IJwtService — sử dụng JsonWebTokenHandler (modern API .NET 10).
///
/// Config đọc từ appsettings.json:
///   JwtSettings:SecretKey               — HMAC-SHA256 signing key
///   JwtSettings:Issuer                  — token issuer
///   JwtSettings:Audience                — token audience
///   JwtSettings:AccessTokenExpiryMinutes — default 60
///   JwtSettings:RefreshTokenExpiryDays  — default 7 (chỉ dùng cho TTL lưu DB)
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly JsonWebTokenHandler _tokenHandler = new();

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    // ── GenerateAccessToken ───────────────────────────────────────────────────
    /// <inheritdoc/>
    public string GenerateAccessToken(JwtUserDto user)
    {
        var key           = GetSigningKey();
        var expiryMinutes = int.TryParse(_config["JwtSettings:AccessTokenExpiryMinutes"], out var m) ? m : 60;

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name,  user.FullName),
                new Claim(ClaimTypes.Role,               user.Role),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
            ]),
            Issuer             = _config["JwtSettings:Issuer"],
            Audience           = _config["JwtSettings:Audience"],
            Expires            = DateTime.UtcNow.AddMinutes(expiryMinutes),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return _tokenHandler.CreateToken(descriptor);
    }

    // ── GenerateRefreshToken ──────────────────────────────────────────────────
    /// <inheritdoc/>
    public string GenerateRefreshToken()
    {
        // 64 bytes → 512 bits entropy — cryptographically secure
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    // ── ValidateTokenAsync ─────────────────────────────────────────────────────
    /// <inheritdoc/>
    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        try
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = GetSigningKey(),
                ValidateIssuer           = true,
                ValidIssuer              = _config["JwtSettings:Issuer"],
                ValidateAudience         = true,
                ValidAudience            = _config["JwtSettings:Audience"],
                ValidateLifetime         = false,   // Bỏ qua lifetime — dùng cho refresh flow
                ClockSkew                = TimeSpan.Zero
            };

            var result = await _tokenHandler.ValidateTokenAsync(token, parameters);

            return result.IsValid
                ? new ClaimsPrincipal(result.ClaimsIdentity)
                : null;
        }
        catch
        {
            return null;
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────────
    private SymmetricSecurityKey GetSigningKey()
    {
        var secretKey = _config["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }
}
