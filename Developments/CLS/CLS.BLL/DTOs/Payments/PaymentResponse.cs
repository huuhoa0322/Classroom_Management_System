namespace CLS.BLL.DTOs.Payments;

/// <summary>
/// Response DTO cho Payment — trả về đầy đủ payment + package + student info.
/// </summary>
public class PaymentResponse
{
    public int Id { get; set; }
    public int StudentPackageId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string RecordedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
