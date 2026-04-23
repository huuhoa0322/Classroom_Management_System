namespace CLS.BLL.DTOs.Payments;

/// <summary>
/// Response DTO cho StudentPackage — hiển thị trên financial dashboard.
/// </summary>
public class StudentPackageResponse
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int RemainingSessions { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
