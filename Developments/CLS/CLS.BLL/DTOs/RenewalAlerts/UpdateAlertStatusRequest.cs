namespace CLS.BLL.DTOs.RenewalAlerts;

/// <summary>
/// Request DTO cho toggle trạng thái alert: pending ↔ consulted.
/// Dùng cho PATCH /api/v1/renewal-alerts/{id}/status.
/// </summary>
public class UpdateAlertStatusRequest
{
    /// <summary>Trạng thái mới: "pending" hoặc "consulted".</summary>
    public string Status { get; set; } = string.Empty;
}
