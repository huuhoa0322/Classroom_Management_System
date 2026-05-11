namespace CLS.BLL.DTOs.ActivityLogs;

/// <summary>DTO trả về cho Activity Log — bao gồm tên user.</summary>
public class ActivityLogResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
