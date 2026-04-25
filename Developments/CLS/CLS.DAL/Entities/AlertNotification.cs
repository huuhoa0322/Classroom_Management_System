namespace CLS.DAL.Entities;

/// <summary>
/// Thông báo cảnh báo gia hạn gói học (Renewal Alert).
/// Bảng: alert_notifications
///
/// Đây là bảng APPEND-ONLY — KHÔNG kế thừa BaseEntity.
/// Không sử dụng soft delete (giống activity_logs).
///
/// Lifecycle:
///   pending → consulted (khi Admin xác nhận đã tư vấn phụ huynh)
///   consulted → pending (nếu cần re-open)
///
/// Tạo bởi: DepletionScanService (UC-10) chạy daily cron job.
/// Đọc bởi: RenewalAlertsController (UC-06) cho Admin review.
/// </summary>
public class AlertNotification
{
    /// <summary>Primary Key — auto-increment.</summary>
    public int Id { get; set; }

    /// <summary>FK tới gói học đang cạn kiệt. Nullable cho thông báo hệ thống chung.</summary>
    public int? StudentPackageId { get; set; }

    /// <summary>Email phụ huynh nhận thông báo.</summary>
    public string TargetEmail { get; set; } = string.Empty;

    /// <summary>Loại thông báo: 'renewal_alert'. Dùng AppConstants.AlertNotificationType.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Nội dung thông báo mô tả tình trạng gói học.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái xử lý: 'pending', 'consulted'.
    /// Dùng AppConstants.AlertNotificationStatus.
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>Thời điểm tạo thông báo (UTC). Set bởi DB hoặc EF.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Thời điểm Admin đánh dấu "Đã tư vấn" (UTC). Null nếu chưa xử lý.</summary>
    public DateTime? ConsultedAt { get; set; }

    // ── Navigation Properties ────────────────────────────────────────────
    /// <summary>Gói học liên quan (kèm Student → Parent).</summary>
    public StudentPackage? StudentPackage { get; set; }
}
