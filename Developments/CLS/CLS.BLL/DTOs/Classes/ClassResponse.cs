namespace CLS.BLL.DTOs.Classes;

/// <summary>
/// Response DTO cho lớp học — bao gồm thông tin mở rộng.
/// </summary>
public class ClassResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    /// <summary>Số học sinh đang active trong lớp.</summary>
    public int StudentCount { get; set; }

    /// <summary>Tổng số buổi học thuộc lớp.</summary>
    public int SessionCount { get; set; }

    public DateTime CreatedAt { get; set; }
}
