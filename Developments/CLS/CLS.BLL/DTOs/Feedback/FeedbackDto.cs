namespace CLS.BLL.DTOs.Feedback;

/// <summary>Response DTO cho 1 feedback record.</summary>
public class FeedbackDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsSlaOverdue { get; set; }
    public DateTime SubmittedAt { get; set; }
}
