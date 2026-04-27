namespace CLS.BLL.DTOs.Attendance;

/// <summary>Response DTO: Thông tin điểm danh 1 học sinh trong 1 buổi.</summary>
public class AttendanceDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime RecordedAt { get; set; }
}
