using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace SafeHabour.Application.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    /// <returns></returns>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;
        
        _logger.LogInformation("User {UserId} connected to NotificationHub with connection {ConnectionId}", userId, connectionId);

        // Join user to their personal group for targeted notifications
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(connectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} added to personal group", userId);
        }

        // Join user to role-based groups if needed
        var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (!string.IsNullOrEmpty(userRole))
        {
            await Groups.AddToGroupAsync(connectionId, $"role_{userRole}");
            _logger.LogInformation("User {UserId} added to role group: {Role}", userId, userRole);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    /// <param name="exception">Exception that caused the disconnection, if any</param>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("User {UserId} disconnected from NotificationHub with connection {ConnectionId}. Exception: {Exception}", 
            userId, connectionId, exception?.Message);

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific notification group (e.g., for job-specific notifications)
    /// </summary>
    /// <param name="groupName">Name of the group to join</param>
    /// <returns></returns>
    public async Task JoinGroup(string groupName)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("User {UserId} joining group: {GroupName}", userId, groupName);
        
        await Groups.AddToGroupAsync(connectionId, groupName);
        await Clients.Caller.SendAsync("JoinedGroup", groupName);
    }

    /// <summary>
    /// Leave a specific notification group
    /// </summary>
    /// <param name="groupName">Name of the group to leave</param>
    /// <returns></returns>
    public async Task LeaveGroup(string groupName)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("User {UserId} leaving group: {GroupName}", userId, groupName);
        
        await Groups.RemoveFromGroupAsync(connectionId, groupName);
        await Clients.Caller.SendAsync("LeftGroup", groupName);
    }

    /// <summary>
    /// Mark notification as read (client can call this)
    /// </summary>
    /// <param name="notificationId">ID of the notification to mark as read</param>
    /// <returns></returns>
    public async Task MarkNotificationAsRead(string notificationId)
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} marked notification {NotificationId} as read", userId, notificationId);
        
        // You can implement database update logic here
        // For now, just acknowledge the action
        await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
    }

    /// <summary>
    /// Get online status for the current user
    /// </summary>
    /// <returns></returns>
    public async Task GetOnlineStatus()
    {
        var userId = Context.UserIdentifier;
        await Clients.Caller.SendAsync("OnlineStatus", new { userId, isOnline = true, timestamp = DateTime.UtcNow });
    }
}
