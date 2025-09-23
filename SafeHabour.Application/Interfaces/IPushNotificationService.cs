using SafeHabour.Models.Enums;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;

namespace SafeHabour.Application.Interfaces;

/// <summary>
/// Service for sending real-time push notifications via SignalR
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Send notification to a specific user
    /// </summary>
    /// <param name="userId">Target user ID</param>
    /// <param name="notification">Notification data</param>
    /// <returns>Success status</returns>
    Task<bool> SendNotificationToUserAsync(string userId, PushNotificationDto notification);

    /// <summary>
    /// Send notification to multiple users
    /// </summary>
    /// <param name="userIds">List of target user IDs</param>
    /// <param name="notification">Notification data</param>
    /// <returns>Success status</returns>
    Task<bool> SendNotificationToUsersAsync(IEnumerable<string> userIds, PushNotificationDto notification);

    /// <summary>
    /// Send notification to all users with a specific role
    /// </summary>
    /// <param name="role">Target role</param>
    /// <param name="notification">Notification data</param>
    /// <returns>Success status</returns>
    Task<bool> SendNotificationToRoleAsync(string role, PushNotificationDto notification);

    /// <summary>
    /// Send notification to a specific group
    /// </summary>
    /// <param name="groupName">Target group name</param>
    /// <param name="notification">Notification data</param>
    /// <returns>Success status</returns>
    Task<bool> SendNotificationToGroupAsync(string groupName, PushNotificationDto notification);

    /// <summary>
    /// Send notification to all connected users (broadcast)
    /// </summary>
    /// <param name="notification">Notification data</param>
    /// <returns>Success status</returns>
    Task<bool> SendBroadcastNotificationAsync(PushNotificationDto notification);

    /// <summary>
    /// Get count of users currently online
    /// </summary>
    /// <returns>Number of connected users</returns>
    Task<int> GetOnlineUsersCountAsync();

    /// <summary>
    /// Check if a specific user is currently online
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>True if user is online, false otherwise</returns>
    Task<bool> IsUserOnlineAsync(string userId);

    /// <summary>
    /// Send a typed notification based on the notification type
    /// This method provides convenience for common notification scenarios
    /// </summary>
    /// <param name="userId">Target user ID</param>
    /// <param name="type">Type of notification</param>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="data">Additional data (optional)</param>
    /// <param name="priority">Notification priority</param>
    /// <returns>Success status</returns>
    Task<bool> SendTypedNotificationAsync(
        string userId,
        NotificationType type,
        string title,
        string message,
        Dictionary<string, object>? data = null,
        NotificationPriority priority = NotificationPriority.Normal);

    /// <summary>
    /// Get paginated notifications for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Pagination and filter parameters</param>
    /// <returns>Paginated notifications</returns>
    Task<PaginatedNotificationsDto> GetUserNotificationsAsync(string userId, GetNotificationsRequest request);

    /// <summary>
    /// Get notification statistics for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Notification statistics</returns>
    Task<NotificationStatsDto> GetUserNotificationStatsAsync(string userId);

    /// <summary>
    /// Mark notifications as read
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="notificationIds">List of notification IDs to mark as read</param>
    /// <returns>Number of notifications marked as read</returns>
    Task<int> MarkNotificationsAsReadAsync(string userId, List<Guid> notificationIds);

    /// <summary>
    /// Clear notifications based on criteria
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Clear criteria</param>
    /// <returns>Number of notifications cleared</returns>
    Task<int> ClearNotificationsAsync(string userId, ClearNotificationsRequest request);
}
