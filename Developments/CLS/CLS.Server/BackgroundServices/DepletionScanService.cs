using CLS.BLL.Interfaces;
using CLS.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CLS.Server.BackgroundServices;

/// <summary>
/// UC-10: Daily Depletion Scan Background Service.
///
/// Chạy tự động:
///   - 1 lần khi startup (sau 10s delay) — cho demo/testing.
///   - Mỗi 24h sau đó (configurable qua appsettings.json "DepletionScan:IntervalHours").
///
/// Quét student_packages có remaining_sessions ≤ 4 hoặc end_date ≤ 14 ngày.
/// Tạo alert_notifications mới → Push SignalR event "NewRenewalAlerts" tới Admin group.
/// </summary>
public class DepletionScanService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<DepletionScanService> _logger;
    private readonly TimeSpan _interval;

    public DepletionScanService(
        IServiceScopeFactory scopeFactory,
        IHubContext<NotificationHub> hubContext,
        ILogger<DepletionScanService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;

        var hours = configuration.GetValue<int>("DepletionScan:IntervalHours", 24);
        _interval = TimeSpan.FromHours(hours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "DepletionScanService started. Interval: {Interval}h. First scan in 10s...",
            _interval.TotalHours);

        // Delay startup scan — đợi hệ thống khởi động xong
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunScanAsync(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RunScanAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var alertService = scope.ServiceProvider.GetRequiredService<IRenewalAlertService>();

            var newAlertCount = await alertService.ScanAndCreateAlertsAsync(ct);

            if (newAlertCount > 0)
            {
                // Push real-time notification tới tất cả Admin đang online
                await _hubContext.Clients.Group("Admins")
                    .SendAsync("NewRenewalAlerts", new { count = newAlertCount }, ct);

                _logger.LogInformation(
                    "Pushed SignalR 'NewRenewalAlerts' to Admins group — {Count} new alerts.",
                    newAlertCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Depletion scan failed. Will retry on next interval.");
        }
    }
}
