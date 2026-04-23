using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

/// <summary>
/// Repository cho Payment — bản ghi thanh toán offline.
/// </summary>
public interface IPaymentRepository : IRepository<Payment>
{
    /// <summary>Lấy danh sách payments của student (phân trang), eager-load details.</summary>
    Task<(List<Payment> Items, int TotalCount)> GetPagedByStudentIdAsync(
        int studentId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Lấy payment theo ID kèm đầy đủ navigation (tracking — cho update status).</summary>
    Task<Payment?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
}
