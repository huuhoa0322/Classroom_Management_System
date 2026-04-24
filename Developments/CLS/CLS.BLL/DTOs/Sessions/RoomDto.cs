namespace CLS.BLL.DTOs.Sessions;

/// <summary>Lightweight DTO cho dropdown phòng học.</summary>
public class RoomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
}
