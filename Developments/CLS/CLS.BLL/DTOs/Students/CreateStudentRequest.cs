namespace CLS.BLL.DTOs.Students;

/// <summary>
/// Request DTO để tạo mới hồ sơ học sinh (CLS-001: Onboard New Student Profiles).
/// Bao gồm cả thông tin phụ huynh — hệ thống sẽ tự động upsert Parent record.
/// </summary>
public class CreateStudentRequest
{
    // ── Thông tin học sinh ───────────────────────────────────────────────
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }

    // ── Thông tin phụ huynh (upsert theo Email) ──────────────────────────
    /// <summary>
    /// Email phụ huynh — BẮT BUỘC (AC2: thiếu email → từ chối lưu).
    /// Đây là kênh duy nhất để gửi thông báo tự động zero-touch.
    /// </summary>
    public string ParentEmail { get; set; } = string.Empty;
    public string ParentFullName { get; set; } = string.Empty;
    public string? ParentPhone { get; set; }
    public string ParentRelationship { get; set; } = string.Empty;
}
