namespace CLS.BLL.DTOs.StudentPackages;

public class PackageResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
