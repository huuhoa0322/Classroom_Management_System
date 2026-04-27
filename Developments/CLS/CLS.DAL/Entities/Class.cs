namespace CLS.DAL.Entities;

/// <summary>
/// Lớp học — nhóm học sinh được giảng dạy cùng nhau.
/// Bảng: classes
/// </summary>
public class Class : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "active";

    /// <summary>FK tới User (Admin) đã tạo lớp.</summary>
    public int CreatedBy { get; set; }

    // ── Navigation Properties ────────────────────────────────────────────
    /// <summary>Admin đã tạo lớp.</summary>
    public User Creator { get; set; } = null!;

    /// <summary>Danh sách buổi học thuộc lớp.</summary>
    public ICollection<Session> Sessions { get; set; } = [];

    /// <summary>Danh sách học sinh thuộc lớp (qua bảng class_students).</summary>
    public ICollection<ClassStudent> ClassStudents { get; set; } = [];
}
