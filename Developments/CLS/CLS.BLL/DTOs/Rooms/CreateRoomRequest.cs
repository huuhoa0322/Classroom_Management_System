namespace CLS.BLL.DTOs.Rooms;

public class CreateRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
}
