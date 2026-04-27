namespace CLS.DAL.Entities;

/// <summary>
/// Bảng trung gian M:N giữa Class và Student.
/// Bảng: class_students
/// </summary>
public class ClassStudent : BaseEntity
{
    /// <summary>FK tới lớp học.</summary>
    public int ClassId { get; set; }

    /// <summary>FK tới học sinh.</summary>
    public int StudentId { get; set; }

    /// <summary>Ngày ghi danh vào lớp.</summary>
    public DateOnly EnrollmentDate { get; set; }

    /// <summary>Trạng thái: active, inactive.</summary>
    public string Status { get; set; } = "active";

    // ── Navigation Properties ────────────────────────────────────────────
    public Class Class { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
