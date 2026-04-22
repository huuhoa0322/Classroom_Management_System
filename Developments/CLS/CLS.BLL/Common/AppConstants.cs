namespace CLS.BLL.Common;

/// <summary>
/// Constants toàn cục cho CLS — tránh magic strings trong Controller và Service.
/// </summary>
public static class AppConstants
{
    // ── API Versioning ────────────────────────────────────────────────────────
    /// <summary>Global route prefix cho tất cả API endpoints.</summary>
    public const string ApiPrefix = "api/v1";

    // ── Route Names ───────────────────────────────────────────────────────────
    public static class Routes
    {
        public const string Auth     = $"{ApiPrefix}/auth";
        public const string Students = $"{ApiPrefix}/students";
        public const string Sessions = $"{ApiPrefix}/class-sessions";
        public const string Rooms    = $"{ApiPrefix}/rooms";
        public const string Teachers = $"{ApiPrefix}/teachers";
        public const string Packages = $"{ApiPrefix}/packages";
        public const string Alerts   = $"{ApiPrefix}/renewal-alerts";
    }

    // ── Public Endpoints (AllowAnonymous) ─────────────────────────────────────
    /// <summary>
    /// Danh sách endpoints KHÔNG yêu cầu JWT (L2 mục 4.3).
    /// Dùng để reference — [AllowAnonymous] vẫn phải đặt trực tiếp trên Controller action.
    /// </summary>
    public static class PublicEndpoints
    {
        /// <summary>POST /api/v1/auth/login — đăng nhập, trả về access + refresh token.</summary>
        public const string Login        = $"{Routes.Auth}/login";

        /// <summary>POST /api/v1/auth/refresh-token — lấy access token mới từ refresh token.</summary>
        public const string RefreshToken = $"{Routes.Auth}/refresh-token";
    }

    // ── Pagination Defaults ───────────────────────────────────────────────────
    public static class Pagination
    {
        public const int DefaultPage     = 1;
        public const int DefaultPageSize = 20;
        public const int MaxPageSize     = 100;
    }

    // ── Claim Types (custom) ──────────────────────────────────────────────────
    public static class ClaimNames
    {
        /// <summary>User ID claim — giá trị của JwtRegisteredClaimNames.Sub.</summary>
        public const string UserId = "sub";
    }

    // ── Student Status ────────────────────────────────────────────────────────
    public static class StudentStatus
    {
        public const string Active   = "active";
        public const string Inactive = "inactive";
    }
}
