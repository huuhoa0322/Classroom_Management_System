using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

/// <summary>
/// Repository cho StudentPackage — gói học của từng học sinh.
/// </summary>
public interface IStudentPackageRepository : IRepository<StudentPackage>
{
    /// <summary>Lấy tất cả gói của một học sinh (kèm TuitionPackage info).</summary>
    Task<List<StudentPackage>> GetByStudentIdAsync(int studentId, CancellationToken ct = default);

    /// <summary>Lấy gói theo ID kèm navigation properties (tracking — cho update).</summary>
    Task<StudentPackage?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Lấy tất cả active student packages kèm Student → Parent (read-only).
    /// Dùng bởi DepletionScanService (UC-10) để batch scan.
    /// </summary>
    Task<List<StudentPackage>> GetActiveWithDetailsAsync(CancellationToken ct = default);
}
