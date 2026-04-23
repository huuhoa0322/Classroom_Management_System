using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

/// <summary>
/// Repository cho TuitionPackage — catalog gói học.
/// </summary>
public interface ITuitionPackageRepository : IRepository<TuitionPackage>
{
    /// <summary>Lấy tất cả gói đang active (cho dropdown chọn gói).</summary>
    Task<List<TuitionPackage>> GetAllActiveAsync(CancellationToken ct = default);
}
