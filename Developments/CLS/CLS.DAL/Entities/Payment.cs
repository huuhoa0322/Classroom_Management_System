namespace CLS.DAL.Entities;

/// <summary>
/// Bản ghi thanh toán offline (chuyển khoản ngân hàng giả lập).
/// Bảng: payments
///
/// Status lifecycle:
///   pending → confirmed (credit sessions)
///   pending / confirmed → failed
///   confirmed → refunded
/// </summary>
public class Payment : BaseEntity
{
    /// <summary>FK tới StudentPackage đang thanh toán.</summary>
    public int StudentPackageId { get; set; }

    /// <summary>FK tới User (Admin) đã ghi nhận thanh toán.</summary>
    public int AdminId { get; set; }

    /// <summary>Số tiền thanh toán (VNĐ).</summary>
    public decimal Amount { get; set; }

    /// <summary>Thời điểm thanh toán (UTC).</summary>
    public DateTime PaymentDate { get; set; }

    /// <summary>Phương thức thanh toán. Giả lập: luôn là "bank_transfer".</summary>
    public string PaymentMethod { get; set; } = "bank_transfer";

    /// <summary>
    /// Trạng thái thanh toán: pending, confirmed, failed, refunded.
    /// Dùng AppConstants.PaymentStatus.
    /// </summary>
    public string Status { get; set; } = "pending";


    // ── Navigation Properties ────────────────────────────────────────────
    /// <summary>Gói học sinh liên kết.</summary>
    public StudentPackage StudentPackage { get; set; } = null!;

    /// <summary>Admin đã ghi nhận thanh toán.</summary>
    public User RecordedBy { get; set; } = null!;
}
