namespace CLS.DAL.Entities;

/// <summary>
/// Đại diện thông tin liên lạc của phụ huynh học sinh.
/// Phụ huynh KHÔNG trực tiếp đăng nhập vào hệ thống — đây là Contact Record thuần túy.
/// Hệ thống sử dụng Email để gửi thông báo tự động (zero-touch notifications).
/// </summary>
public class Parent : BaseEntity
{
    /// <summary>Họ và tên đầy đủ của phụ huynh.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email phụ huynh — UNIQUE, bắt buộc.
    /// Đây là kênh duy nhất để hệ thống gửi thông báo tự động.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Số điện thoại liên lạc.</summary>
    public string? Phone { get; set; }

    /// <summary>Quan hệ với học sinh (Bố, Mẹ, Người giám hộ...).</summary>
    public string Relationship { get; set; } = string.Empty;

    // ── Navigation Properties ────────────────────────────────────────────
    /// <summary>Danh sách học sinh của phụ huynh này.</summary>
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
