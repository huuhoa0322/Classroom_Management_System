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

    // ── User Roles ────────────────────────────────────────────────────────────
    public static class AppRoles
    {
        public const string Admin   = "Admin";
        public const string Teacher = "Teacher";
    }

    // ── Student Status ────────────────────────────────────────────────────────
    public static class StudentStatus
    {
        public const string Active   = "active";
        public const string Inactive = "inactive";
    }

    // ── Class Status ──────────────────────────────────────────────────────────
    public static class ClassStatus
    {
        public const string Active   = "active";
        public const string Inactive = "inactive";
    }

    // ── ClassStudent (Enrollment) Status ──────────────────────────────────────
    public static class ClassStudentStatus
    {
        public const string Active   = "active";
        public const string Inactive = "inactive";
    }

    // ── Payment Status (CLS-003) ──────────────────────────────────────────────
    public static class PaymentStatus
    {
        public const string Pending   = "pending";
        public const string Confirmed = "confirmed";
        public const string Failed    = "failed";
        public const string Refunded  = "refunded";
    }

    // ── Student Package Status (CLS-003) ──────────────────────────────────────
    public static class StudentPackageStatus
    {
        public const string PendingPayment = "pending_payment";
        public const string Active         = "active";
        public const string Depleted       = "depleted";
        public const string Archived       = "archived";
    }

    // ── Payment Method (CLS-003 — giả lập) ───────────────────────────────────
    public static class PaymentMethod
    {
        public const string BankTransfer = "bank_transfer";
    }

    // ── Session Status (CLS-004 + CLS-005) ────────────────────────────────────
    public static class SessionStatus
    {
        public const string Scheduled  = "scheduled";
        public const string InProgress = "in_progress";
        public const string Completed  = "completed";
        public const string Cancelled  = "cancelled";
    }

    // ── Operating Hours (CLS-004 — giờ hoạt động trung tâm) ───────────────────
    public static class OperatingHours
    {
        /// <summary>Giờ mở cửa (08:00).</summary>
        public const int OpenHour  = 8;
        /// <summary>Giờ đóng cửa (21:00).</summary>
        public const int CloseHour = 21;
    }

    // ── Alert Notification Status (CLS-006) ───────────────────────────────────
    public static class AlertNotificationStatus
    {
        public const string Pending   = "pending";
        public const string Consulted = "consulted";
    }

    // ── Alert Notification Type (CLS-006 + CLS-010) ───────────────────────────
    public static class AlertNotificationType
    {
        public const string RenewalAlert = "renewal_alert";
    }

    // ── Depletion Scan Thresholds (CLS-010) ───────────────────────────────────
    /// <summary>Ngưỡng thông báo gia hạn gói học — dùng bởi DepletionScanService.</summary>
    public static class DepletionThresholds
    {
        /// <summary>Số buổi còn lại tối thiểu để trigger alert.</summary>
        public const int MinSessions = 4;
        /// <summary>Số ngày còn lại tối thiểu trước end_date để trigger alert.</summary>
        public const int MinDays = 14;
    }

    // ── Attendance Status (UC-08) ─────────────────────────────────────────────
    public static class AttendanceStatus
    {
        public const string Present = "present";
        public const string Absent  = "absent";
        public const string Late    = "late";
    }

    // ── SLA Configuration (UC-09) ─────────────────────────────────────────────
    public static class Sla
    {
        /// <summary>Thời hạn submit feedback sau khi buổi học kết thúc (12 giờ).</summary>
        public const int FeedbackWindowHours = 12;
    }

    // ── Room Status ───────────────────────────────────────────────────────────
    public static class RoomStatus
    {
        public const string Active   = "active";
        public const string Inactive = "inactive";
    }

    // ── Tuition Package Status ────────────────────────────────────────────────
    public static class TuitionPackageStatus
    {
        public const string Active   = "active";
        public const string Inactive = "inactive";
    }

    // ── User Status ───────────────────────────────────────────────────────────
    public static class UserAccountStatus
    {
        public const string Active   = "active";
        public const string Inactive = "inactive";
    }

    // ── Activity Log Action Types ─────────────────────────────────────────
    /// <summary>Các action_type chuẩn dùng cho bảng activity_logs.</summary>
    public static class ActionTypes
    {
        public const string Create       = "create";
        public const string Update       = "update";
        public const string Delete       = "delete";
        public const string Login        = "login";
        public const string StatusChange = "status_change";
    }
}
