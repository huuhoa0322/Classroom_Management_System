using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

/// <summary>
/// Repository cho Attendance — điểm danh (UC-08).
/// </summary>
public interface IAttendanceRepository : IRepository<Attendance>
{
    /// <summary>Lấy danh sách điểm danh của 1 buổi học.</summary>
    Task<List<Attendance>> GetBySessionIdAsync(int sessionId, CancellationToken ct = default);

    /// <summary>Kiểm tra buổi học đã có bản ghi điểm danh chưa.</summary>
    Task<bool> HasAttendanceAsync(int sessionId, CancellationToken ct = default);

    /// <summary>Thêm batch bản ghi điểm danh (submit cả buổi cùng lúc).</summary>
    Task AddRangeAsync(IEnumerable<Attendance> records, CancellationToken ct = default);

    /// <summary>Xóa toàn bộ bản ghi điểm danh của 1 buổi (hard-delete để ghi lại).</summary>
    Task DeleteBySessionIdAsync(int sessionId, CancellationToken ct = default);
}
