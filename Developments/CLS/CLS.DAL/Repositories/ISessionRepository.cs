using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

/// <summary>
/// Repository cho Session — buổi học (CLS-004 + CLS-005).
/// </summary>
public interface ISessionRepository : IRepository<Session>
{
    /// <summary>Lấy danh sách sessions (phân trang), eager-load details.</summary>
    Task<(List<Session> Items, int TotalCount)> GetPagedAllAsync(
        int page, int pageSize, CancellationToken ct = default);

    /// <summary>Lấy session theo ID kèm navigation (tracking — cho update).</summary>
    Task<Session?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);

    /// <summary>Kiểm tra xung đột Teacher (CLS-005 AC1).</summary>
    Task<bool> HasTeacherConflictAsync(
        int teacherId, DateTime startTime, DateTime endTime,
        int? excludeSessionId = null, CancellationToken ct = default);

    /// <summary>Lấy buổi học đang xung đột với Teacher để trả chi tiết cho UI.</summary>
    Task<Session?> GetTeacherConflictAsync(
        int teacherId, DateTime startTime, DateTime endTime,
        int? excludeSessionId = null, CancellationToken ct = default);

    /// <summary>Kiểm tra xung đột Room (CLS-005 AC2).</summary>
    Task<bool> HasRoomConflictAsync(
        int roomId, DateTime startTime, DateTime endTime,
        int? excludeSessionId = null, CancellationToken ct = default);

    /// <summary>Lấy buổi học đang xung đột với Room để trả chi tiết cho UI.</summary>
    Task<Session?> GetRoomConflictAsync(
        int roomId, DateTime startTime, DateTime endTime,
        int? excludeSessionId = null, CancellationToken ct = default);
}
