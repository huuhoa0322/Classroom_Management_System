namespace CLS.BLL.DTOs.Sessions;

/// <summary>Request DTO: Tạo buổi học mới (CLS-004 AC1).</summary>
public class CreateSessionRequest
{
    public int ClassId { get; set; }
    public int TeacherId { get; set; }
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
