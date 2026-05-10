namespace CLS.BLL.DTOs.StudentPackages;

public class CreatePackageRequest
{
    public string Name { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
}
