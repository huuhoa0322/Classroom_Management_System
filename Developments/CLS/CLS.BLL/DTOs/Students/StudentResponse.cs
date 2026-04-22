namespace CLS.BLL.DTOs.Students;

/// <summary>
/// Response DTO trả về thông tin học sinh kèm phụ huynh.
/// Không expose trường nhạy cảm nào.
/// </summary>
public class StudentResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // ── Thông tin phụ huynh (flatten — không nest object) ────────────────
    public int ParentId { get; set; }
    public string ParentFullName { get; set; } = string.Empty;
    public string ParentEmail { get; set; } = string.Empty;
    public string? ParentPhone { get; set; }
    public string ParentRelationship { get; set; } = string.Empty;
}
