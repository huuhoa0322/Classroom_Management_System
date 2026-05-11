using CLS.BLL.Common;
using CLS.BLL.DTOs.ActivityLogs;

namespace CLS.BLL.Interfaces;

/// <summary>Service interface cho Activity Logs — đọc + ghi log.</summary>
public interface IActivityLogService
{
    /// <summary>Lấy danh sách log phân trang với bộ lọc.</summary>
    Task<PagedResult<ActivityLogResponse>> GetAllAsync(
        int page, int pageSize,
        int? userId = null,
        string? actionType = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default);

    /// <summary>Ghi một log entry mới (append-only).</summary>
    Task LogAsync(int userId, string actionType, string? description = null, CancellationToken ct = default);
}
