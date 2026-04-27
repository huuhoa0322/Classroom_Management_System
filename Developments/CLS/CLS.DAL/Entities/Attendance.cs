namespace CLS.DAL.Entities;

/// <summary>
/// Bản ghi điểm danh — liên kết 1 Student với 1 Session.
/// Bảng: attendances
///
/// Status: present, absent, late
/// Constraint: UNIQUE(session_id, student_id) — mỗi học sinh chỉ điểm danh 1 lần/buổi.
/// </summary>
public class Attendance : BaseEntity
{
    /// <summary>FK tới buổi học.</summary>
    public int SessionId { get; set; }

    /// <summary>FK tới học sinh.</summary>
    public int StudentId { get; set; }

    /// <summary>Trạng thái điểm danh: present, absent, late.</summary>
    public string Status { get; set; } = "absent";

    /// <summary>Ghi chú tùy chọn của giáo viên.</summary>
    public string? Note { get; set; }

    /// <summary>Thời điểm ghi nhận điểm danh (UTC).</summary>
    public DateTime RecordedAt { get; set; }

    // ── Navigation Properties ────────────────────────────────────────────
    public Session Session { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
