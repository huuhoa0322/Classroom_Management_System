using CLS.BLL.Common;
using CLS.BLL.DTOs.RenewalAlerts;

namespace CLS.BLL.Interfaces;

/// <summary>
/// Service interface cho Retention Management — UC-06 (Review Alerts) + UC-10 (Depletion Scan).
/// </summary>
public interface IRenewalAlertService
{
    /// <summary>Lấy danh sách cảnh báo gia hạn (phân trang, sort, filter).</summary>
    Task<PagedResult<RenewalAlertResponse>> GetAlertsAsync(
        int page, int pageSize,
        string? status, string? sortBy, string? sortDir,
        CancellationToken ct = default);

    /// <summary>Toggle trạng thái alert: pending ↔ consulted.</summary>
    Task<ServiceResult<RenewalAlertResponse>> UpdateAlertStatusAsync(
        int alertId, UpdateAlertStatusRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// UC-10: Scan student_packages sắp hết hạn/cạn buổi → tạo renewal alerts.
    /// Trả về số alerts mới được tạo.
    /// </summary>
    Task<int> ScanAndCreateAlertsAsync(CancellationToken ct = default);
}
