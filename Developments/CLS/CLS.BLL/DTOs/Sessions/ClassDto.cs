namespace CLS.BLL.DTOs.Sessions;

/// <summary>Lightweight DTO cho dropdown lớp học.</summary>
public class ClassDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
