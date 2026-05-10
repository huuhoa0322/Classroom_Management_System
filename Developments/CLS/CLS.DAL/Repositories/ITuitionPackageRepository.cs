using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

/// <summary>
/// Repository cho TuitionPackage — catalog gói học.
/// </summary>
public interface ITuitionPackageRepository : IRepository<TuitionPackage>
{
    /// <summary>Lấy tất cả gói đang active (cho dropdown chọn gói).</summary>
    Task<List<TuitionPackage>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>Lấy danh sách gói phân trang, include StudentPackages để đếm.</summary>
    Task<(List<TuitionPackage> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Kiểm tra tên gói đã tồn tại (exclude id khi update).</summary>
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default);
}
