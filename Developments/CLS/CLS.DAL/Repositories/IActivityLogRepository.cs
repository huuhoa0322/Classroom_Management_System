using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

/// <summary>
/// Repository interface cho activity_logs — READ + APPEND only.
/// Không có Update, Delete (bảng append-only).
/// </summary>
public interface IActivityLogRepository
{
    /// <summary>Lấy danh sách log phân trang với bộ lọc tùy chọn.</summary>
    Task<(List<ActivityLog> Items, int Total)> GetPagedAsync(
        int page, int pageSize,
        int? userId = null,
        string? actionType = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default);

    /// <summary>Thêm một log entry mới (append-only).</summary>
    Task AddAsync(ActivityLog log, CancellationToken ct = default);

    /// <summary>Lưu thay đổi vào database.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
