namespace CLS.BLL.DTOs.Feedback;

/// <summary>DTO cho trang danh sách feedback của 1 session (FeedbackListPage).</summary>
public class FeedbackListDto
{
    public int SessionId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    /// <summary>Deadline SLA = EndTime + 12h.</summary>
    public DateTime SlaDeadline { get; set; }

    /// <summary>True nếu hiện tại đã quá SLA deadline.</summary>
    public bool IsSlaExpired { get; set; }

    /// <summary>Danh sách học sinh + trạng thái đánh giá.</summary>
    public List<StudentFeedbackSummary> Students { get; set; } = [];

    /// <summary>Feedbacks đã submit (nếu có).</summary>
    public List<FeedbackDto>? ExistingFeedbacks { get; set; }
}

/// <summary>Tóm tắt trạng thái feedback cho 1 học sinh trong list.</summary>
public class StudentFeedbackSummary
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;

    /// <summary>True nếu đã có feedback.</summary>
    public bool HasFeedback { get; set; }

    /// <summary>Score nếu đã feedback (null nếu chưa).</summary>
    public int? Score { get; set; }
}
