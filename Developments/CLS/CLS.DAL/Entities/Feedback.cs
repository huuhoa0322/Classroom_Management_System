namespace CLS.DAL.Entities;

/// <summary>
/// Đánh giá học tập — Teacher đánh giá 1 Student trong 1 Session.
/// Bảng: feedbacks
///
/// Score: 1–10
/// SLA: 12h sau khi buổi học kết thúc
/// </summary>
public class Feedback : BaseEntity
{
    /// <summary>FK tới buổi học.</summary>
    public int SessionId { get; set; }

    /// <summary>FK tới học sinh.</summary>
    public int StudentId { get; set; }

    /// <summary>FK tới giáo viên đánh giá.</summary>
    public int TeacherId { get; set; }

    /// <summary>Nội dung nhận xét (bắt buộc, max 1000 ký tự).</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Điểm đánh giá 1–10.</summary>
    public int? Score { get; set; }

    /// <summary>Thời điểm submit (UTC).</summary>
    public DateTime SubmittedAt { get; set; }

    /// <summary>True nếu submit sau SLA 12h.</summary>
    public bool IsSlaOverdue { get; set; }

    // ── Navigation Properties ────────────────────────────────────────────
    public Session Session { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public User Teacher { get; set; } = null!;
}
