namespace CLS.BLL.DTOs.Feedback;

/// <summary>Request DTO để submit feedback cho 1 student.</summary>
public class SubmitFeedbackRequest
{
    /// <summary>ID của học sinh.</summary>
    public int StudentId { get; set; }

    /// <summary>Điểm đánh giá 1–10.</summary>
    public int Score { get; set; }

    /// <summary>Nội dung nhận xét (bắt buộc, max 1000 ký tự).</summary>
    public string Content { get; set; } = string.Empty;
}
