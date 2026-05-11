namespace CLS.DAL.Entities;

/// <summary>
/// Lịch sử hoạt động hệ thống (audit trail).
/// Bảng: activity_logs
///
/// Đây là bảng APPEND-ONLY — KHÔNG kế thừa BaseEntity.
/// Không sử dụng soft delete, không có updated_at.
///
/// Action types chuẩn: create, update, delete, login, logout, status_change.
/// Ghi bởi: Các Service khi thực hiện thao tác nghiệp vụ.
/// Đọc bởi: ActivityLogsController cho Admin review.
/// </summary>
public class ActivityLog
{
    /// <summary>Primary Key — auto-increment.</summary>
    public int Id { get; set; }

    /// <summary>FK tới user thực hiện hành động.</summary>
    public int UserId { get; set; }

    /// <summary>Loại hành động: create, update, delete, login, logout, status_change.</summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>Mô tả chi tiết hành động (nullable).</summary>
    public string? Description { get; set; }

    /// <summary>Thời điểm thực hiện hành động (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    // ── Navigation Properties ────────────────────────────────────────────
    /// <summary>User thực hiện hành động.</summary>
    public User? User { get; set; }
}
