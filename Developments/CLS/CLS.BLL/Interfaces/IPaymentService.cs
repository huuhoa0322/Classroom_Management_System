using CLS.BLL.Common;
using CLS.BLL.DTOs.Payments;

namespace CLS.BLL.Interfaces;

public interface IPaymentService
{
    /// <summary>Ghi nhận thanh toán offline mới (status = pending).</summary>
    Task<ServiceResult<PaymentResponse>> RecordPaymentAsync(RecordPaymentRequest request, int adminUserId, CancellationToken ct = default);

    /// <summary>Cập nhật trạng thái thanh toán (confirm / fail / refund).</summary>
    Task<ServiceResult<PaymentResponse>> UpdatePaymentStatusAsync(int paymentId, UpdatePaymentStatusRequest request, CancellationToken ct = default);

    /// <summary>Lấy lịch sử thanh toán của học sinh (phân trang).</summary>
    Task<PagedResult<PaymentResponse>> GetPaymentsByStudentAsync(int studentId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Lấy toàn bộ lịch sử thanh toán (phân trang, không lọc student).</summary>
    Task<PagedResult<PaymentResponse>> GetAllPaymentsAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Lấy danh sách gói học của học sinh.</summary>
    Task<IEnumerable<StudentPackageResponse>> GetStudentPackagesAsync(int studentId, CancellationToken ct = default);

    /// <summary>Lấy catalog gói học (active only).</summary>
    Task<IEnumerable<TuitionPackageDto>> GetAvailablePackagesAsync(CancellationToken ct = default);
}
