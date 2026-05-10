namespace CLS.BLL.DTOs;

/// <summary>Response cho Dashboard Statistics.</summary>
public class DashboardStatsResponse
{
    public int TotalStudents { get; set; }
    public int TotalClasses { get; set; }
    public int TotalTeachers { get; set; }
    public int PendingAlerts { get; set; }
    public decimal TotalRevenue { get; set; }
    public int UpcomingSessions { get; set; }
}
