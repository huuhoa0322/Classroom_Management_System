namespace CLS.DAL.Entities;

/// <summary>
/// Gói học đã được gán cho học sinh. Theo dõi balance sessions.
/// Bảng: student_packages
///
/// Status lifecycle:
///   pending_payment → active (khi confirm payment)
///   active → depleted (khi remaining_sessions = 0)
///   active / pending_payment → archived (khi payment failed/refunded hoặc student drop out)
/// </summary>
public class StudentPackage : BaseEntity
{
    /// <summary>FK tới học sinh.</summary>
    public int StudentId { get; set; }

    /// <summary>FK tới gói học từ catalog.</summary>
    public int PackageId { get; set; }

    /// <summary>Ngày bắt đầu hiệu lực gói.</summary>
    public DateOnly StartDate { get; set; }

    /// <summary>Ngày hết hạn gói.</summary>
    public DateOnly EndDate { get; set; }

    /// <summary>Tổng số buổi học trong gói (copy từ TuitionPackage lúc tạo).</summary>
    public int TotalSessions { get; set; }

    /// <summary>Số buổi còn lại. Giảm khi điểm danh Present/Late.</summary>
    public int RemainingSessions { get; set; }

    /// <summary>
    /// Trạng thái gói: pending_payment, active, depleted, archived.
    /// Dùng AppConstants.StudentPackageStatus.
    /// </summary>
    public string Status { get; set; } = "pending_payment";

    // ── Navigation Properties ────────────────────────────────────────────
    /// <summary>Học sinh sở hữu gói này.</summary>
    public Student Student { get; set; } = null!;

    /// <summary>Gói học từ catalog.</summary>
    public TuitionPackage Package { get; set; } = null!;

    /// <summary>Các bản ghi thanh toán liên kết với gói này.</summary>
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
