namespace CLS.BLL.DTOs.Sessions;

/// <summary>Request DTO: Cập nhật buổi học (CLS-004).</summary>
public class UpdateSessionRequest
{
    public int ClassId { get; set; }
    public int TeacherId { get; set; }
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
