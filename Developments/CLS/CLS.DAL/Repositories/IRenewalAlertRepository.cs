using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

/// <summary>
/// Repository cho AlertNotification — cảnh báo gia hạn gói học (CLS-006 + CLS-010).
/// Không extend IRepository&lt;T&gt; vì AlertNotification không kế thừa BaseEntity.
/// </summary>
public interface IRenewalAlertRepository
{
    /// <summary>Lấy danh sách alerts phân trang, kèm Student + Parent info.</summary>
    Task<List<AlertNotification>> GetAlertsPagedAsync(
        int page, int pageSize,
        string? statusFilter,
        string? sortBy, string? sortDir,
        CancellationToken ct = default);

    /// <summary>Đếm tổng số alerts (có lọc status nếu cần).</summary>
    Task<int> CountAsync(string? statusFilter, CancellationToken ct = default);

    /// <summary>Lấy alert theo ID kèm navigation properties (tracking — cho update).</summary>
    Task<AlertNotification?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Kiểm tra đã có alert cho StudentPackage chưa (tránh duplicate).</summary>
    Task<bool> ExistsForPackageAsync(int studentPackageId, string status, CancellationToken ct = default);

    /// <summary>
    /// Lấy tất cả StudentPackageId đã có alert BẤT KỲ status (batch query — tránh N+1).
    /// Đảm bảo mỗi gói chỉ tạo 1 alert duy nhất.
    /// Dùng bởi ScanAndCreateAlertsAsync.
    /// </summary>
    Task<HashSet<int>> GetExistingAlertPackageIdsAsync(CancellationToken ct = default);

    /// <summary>Thêm alert mới.</summary>
    Task AddAsync(AlertNotification entity, CancellationToken ct = default);

    /// <summary>Lấy alerts chưa gửi email (email_sent_at IS NULL) — batch cho EmailDispatchService.</summary>
    Task<List<AlertNotification>> GetUnsentForDispatchAsync(int batchSize, CancellationToken ct = default);

    /// <summary>Thêm nhiều alerts cùng lúc (batch insert từ depletion scan).</summary>
    Task AddRangeAsync(IEnumerable<AlertNotification> entities, CancellationToken ct = default);

    /// <summary>Cập nhật alert (tracking).</summary>
    void Update(AlertNotification entity);

    /// <summary>Lưu thay đổi.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
