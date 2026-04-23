namespace CLS.BLL.DTOs.Payments;

/// <summary>
/// Response DTO cho TuitionPackage catalog item (dropdown chọn gói).
/// </summary>
public class TuitionPackageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
}
