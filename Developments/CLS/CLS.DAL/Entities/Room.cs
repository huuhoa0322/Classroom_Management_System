namespace CLS.DAL.Entities;

/// <summary>
/// Phòng học vật lý.
/// Bảng: rooms
/// </summary>
public class Room : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Status { get; set; } = "active";
}
