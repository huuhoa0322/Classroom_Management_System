namespace CLS.BLL.DTOs.Feedback;

/// <summary>DTO cho trang form feedback 1 student (FeedbackFormPage).</summary>
public class StudentFeedbackDto
{
    public int SessionId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    /// <summary>Deadline SLA = EndTime + 12h.</summary>
    public DateTime SlaDeadline { get; set; }
    public bool IsSlaExpired { get; set; }

    /// <summary>Feedback đã submit (null nếu chưa).</summary>
    public FeedbackDto? ExistingFeedback { get; set; }
}
