using CLS.BLL.DTOs;

namespace CLS.BLL.Interfaces;

/// <summary>Service tổng hợp thống kê Dashboard cho Admin.</summary>
public interface IDashboardService
{
    /// <summary>Lấy tổng hợp thống kê hệ thống.</summary>
    Task<DashboardStatsResponse> GetStatsAsync(CancellationToken ct = default);
}
