namespace CLS.DAL.Entities;

/// <summary>
/// Đại diện hồ sơ học sinh trong hệ thống CLS.
/// Lifecycle: active → inactive (Dropped Out). Soft-delete thông qua IsDeleted.
/// </summary>
public class Student : BaseEntity
{
    /// <summary>FK tới phụ huynh phụ trách. Bắt buộc.</summary>
    public int ParentId { get; set; }

    /// <summary>Họ và tên đầy đủ của học sinh.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Ngày sinh (tuỳ chọn).</summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Trạng thái vòng đời học sinh.
    /// Các giá trị hợp lệ: 'active', 'inactive'.
    /// Dùng AppConstants.StudentStatus để tránh magic strings.
    /// </summary>
    public string Status { get; set; } = "active";

    /// <summary>Thời điểm học sinh bắt đầu theo học (UTC).</summary>
    public DateTime EnrolledAt { get; set; }

    // ── Navigation Properties ────────────────────────────────────────────
    /// <summary>Phụ huynh phụ trách học sinh này.</summary>
    public Parent Parent { get; set; } = null!;
}
