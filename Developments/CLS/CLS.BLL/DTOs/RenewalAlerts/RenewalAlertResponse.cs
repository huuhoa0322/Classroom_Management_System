namespace CLS.BLL.DTOs.RenewalAlerts;

/// <summary>
/// Response DTO cho Renewal Alert — flatten từ AlertNotification + StudentPackage + Student + Parent.
/// Dùng cho GET /api/v1/renewal-alerts.
/// </summary>
public class RenewalAlertResponse
{
    public int Id { get; set; }

    // ── Student Info ─────────────────────────────────────────────────────────
    public string StudentName { get; set; } = string.Empty;

    // ── Parent Contact ───────────────────────────────────────────────────────
    public string ParentName { get; set; } = string.Empty;
    public string ParentEmail { get; set; } = string.Empty;
    public string? ParentPhone { get; set; }

    // ── Package Info ─────────────────────────────────────────────────────────
    public string PackageName { get; set; } = string.Empty;
    public int RemainingSessions { get; set; }

    /// <summary>Số ngày còn lại trước khi gói hết hạn.</summary>
    public int RemainingDays { get; set; }

    // ── Alert Meta ───────────────────────────────────────────────────────────
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ConsultedAt { get; set; }
}
