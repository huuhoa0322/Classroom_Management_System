namespace CLS.BLL.DTOs.Payments;

/// <summary>
/// Request DTO để cập nhật trạng thái thanh toán (confirm / fail / refund).
/// </summary>
public class UpdatePaymentStatusRequest
{
    /// <summary>
    /// Trạng thái mới: confirmed, failed, refunded.
    /// Dùng AppConstants.PaymentStatus.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
