namespace CLS.BLL.DTOs.Payments;

/// <summary>
/// Request DTO để ghi nhận thanh toán offline (CLS-003 AC1).
/// </summary>
public class RecordPaymentRequest
{
    /// <summary>ID học sinh cần ghi thanh toán.</summary>
    public int StudentId { get; set; }

    /// <summary>ID gói học từ catalog.</summary>
    public int TuitionPackageId { get; set; }

    /// <summary>Số tiền thanh toán (VNĐ).</summary>
    public decimal AmountPaid { get; set; }

    /// <summary>Ghi chú tuỳ chọn (thông tin chuyển khoản...).</summary>
    public string? Note { get; set; }
}
