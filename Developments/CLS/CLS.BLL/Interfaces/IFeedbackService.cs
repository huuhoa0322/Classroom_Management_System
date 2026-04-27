using CLS.BLL.Common;
using CLS.BLL.DTOs.Feedback;

namespace CLS.BLL.Interfaces;

/// <summary>
/// Service interface cho Academic Quality Assurance (UC-09).
/// </summary>
public interface IFeedbackService
{
    /// <summary>Lấy danh sách HS + trạng thái đánh giá cho 1 session.</summary>
    Task<ServiceResult<FeedbackListDto>> GetFeedbackListAsync(
        int sessionId, int teacherId, CancellationToken ct = default);

    /// <summary>Lấy feedback detail cho 1 student trong 1 session.</summary>
    Task<ServiceResult<StudentFeedbackDto>> GetStudentFeedbackAsync(
        int sessionId, int studentId, int teacherId, CancellationToken ct = default);

    /// <summary>Submit feedback cho 1 student trong 1 session.</summary>
    Task<ServiceResult<object?>> SubmitFeedbackAsync(
        int sessionId, int teacherId, SubmitFeedbackRequest request, CancellationToken ct = default);
}
