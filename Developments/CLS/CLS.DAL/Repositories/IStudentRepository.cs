using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    /// <summary>
    /// Lấy danh sách học sinh có phân trang.
    /// Trả về (items, totalCount) — PagedResult được xây dựng ở Service layer.
    /// </summary>
    Task<(List<Student> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? statusFilter, CancellationToken ct = default);

    /// <summary>Lấy Student kèm thông tin Parent (Include) — AsNoTracking (read-only).</summary>
    Task<Student?> GetWithParentAsync(int id, CancellationToken ct = default);

    /// <summary>Lấy Student kèm Parent CÓ tracking — dùng cho Update flow.</summary>
    Task<Student?> GetWithParentForUpdateAsync(int id, CancellationToken ct = default);
}
