using CLS.BLL.Common;
using CLS.BLL.Interfaces;
using CLS.BLL.Services;
using CLS.DAL.Repositories;

namespace CLS.Server.BackgroundServices;

/// <summary>
/// UC-11: Email Dispatch Background Service.
///
/// Chạy tự động:
///   - 1 lần khi startup (sau 15s delay).
///   - Mỗi N phút sau đó (configurable qua appsettings.json "EmailSettings:DispatchIntervalMinutes").
///
/// Quét alert_notifications có status = 'pending' → render HTML → gửi email → cập nhật status.
/// </summary>
public class EmailDispatchService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailDispatchService> _logger;
    private readonly TimeSpan _interval;
    private readonly int _batchSize;

    public EmailDispatchService(
        IServiceScopeFactory scopeFactory,
        ILogger<EmailDispatchService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var minutes = configuration.GetValue("EmailSettings:DispatchIntervalMinutes", 5);
        _interval = TimeSpan.FromMinutes(minutes);
        _batchSize = configuration.GetValue("EmailSettings:BatchSize", 50);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "EmailDispatchService started. Interval: {Interval}min, BatchSize: {BatchSize}. First dispatch in 15s...",
            _interval.TotalMinutes, _batchSize);

        // Delay startup — đợi hệ thống khởi động xong
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await DispatchPendingEmailsAsync(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task DispatchPendingEmailsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var alertRepo = scope.ServiceProvider.GetRequiredService<IRenewalAlertRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Query alerts chưa gửi email (email_sent_at IS NULL)
            var unsentAlerts = await alertRepo.GetUnsentForDispatchAsync(_batchSize, ct);

            if (unsentAlerts.Count == 0)
            {
                _logger.LogDebug("No unsent alerts to dispatch.");
                return;
            }

            _logger.LogInformation(
                "Found {Count} unsent alerts to dispatch.",
                unsentAlerts.Count);

            int sentCount = 0, failedCount = 0;

            foreach (var alert in unsentAlerts)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    // Extract data for template
                    var parentName = alert.StudentPackage?.Student?.Parent?.FullName ?? "Phụ huynh";
                    var studentName = alert.StudentPackage?.Student?.FullName ?? "Học sinh";
                    var packageName = alert.StudentPackage?.Package?.Name ?? "Gói học";
                    var remainingSessions = alert.StudentPackage?.RemainingSessions ?? 0;

                    // Render HTML template
                    var subject = $"[CLS] Thông báo gia hạn gói học - {studentName}";
                    var htmlBody = EmailTemplateService.RenderRenewalAlert(
                        parentName, studentName, packageName, remainingSessions, alert.Message);

                    // Send email
                    var success = await emailService.SendEmailAsync(
                        alert.TargetEmail, subject, htmlBody, ct);

                    if (success)
                    {
                        // Chỉ set email_sent_at — KHÔNG thay đổi status (giữ pending cho Admin)
                        alert.EmailSentAt = DateTime.UtcNow;
                        alertRepo.Update(alert);
                        sentCount++;
                    }
                    else
                    {
                        // Email thất bại → giữ email_sent_at = NULL → retry lần dispatch sau
                        failedCount++;
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex,
                        "Failed to process alert #{AlertId} for {Email}",
                        alert.Id, alert.TargetEmail);

                    // Không update gì → giữ email_sent_at = NULL → retry lần sau
                    failedCount++;
                }
            }

            // Batch save
            await alertRepo.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Email dispatch completed. Sent: {Sent}, Failed: {Failed}, Total: {Total}",
                sentCount, failedCount, unsentAlerts.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Email dispatch batch failed. Will retry on next interval.");
        }
    }
}
