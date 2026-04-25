using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CLS.Server.Hubs;

/// <summary>
/// SignalR Hub cho real-time notifications.
/// Admin clients kết nối sau khi login → nhận push events khi có renewal alerts mới.
///
/// Events:
///   - "NewRenewalAlerts" → { count: int } — số alerts mới được tạo bởi DepletionScanService.
/// </summary>
[Authorize(Roles = "Admin")]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger) => _logger = logger;

    public override async Task OnConnectedAsync()
    {
        // Thêm Admin vào group "Admins" để push targeted
        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        _logger.LogInformation(
            "Admin {UserId} connected to NotificationHub (ConnectionId: {ConnId})",
            Context.UserIdentifier, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
        _logger.LogInformation(
            "Admin {UserId} disconnected from NotificationHub",
            Context.UserIdentifier);
        await base.OnDisconnectedAsync(exception);
    }
}
