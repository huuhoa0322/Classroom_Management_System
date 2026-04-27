using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

/// <summary>
/// Repository interface cho bảng feedbacks (UC-09).
/// </summary>
public interface IFeedbackRepository
{
    /// <summary>Lấy tất cả feedbacks của 1 session (include Student).</summary>
    Task<List<Feedback>> GetBySessionIdAsync(int sessionId, CancellationToken ct = default);

    /// <summary>Lấy feedback cho 1 student trong 1 session.</summary>
    Task<Feedback?> GetBySessionAndStudentAsync(int sessionId, int studentId, CancellationToken ct = default);

    /// <summary>Kiểm tra session đã có feedback cho 1 student chưa.</summary>
    Task<bool> HasFeedbackAsync(int sessionId, int studentId, CancellationToken ct = default);

    /// <summary>Thêm 1 bản ghi feedback.</summary>
    Task AddAsync(Feedback entity, CancellationToken ct = default);

    /// <summary>Xóa feedback theo ID (hard-delete cho re-submit).</summary>
    void Delete(Feedback entity);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
