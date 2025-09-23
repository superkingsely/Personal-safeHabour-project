using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SafeHabour.Application.Hubs;
using SafeHabour.Application.Interfaces;
using SafeHabour.Data.Data;
using SafeHabour.Data.Entities;
using SafeHabour.Models.Enums;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;
using System.Text.Json;

namespace SafeHabour.Application.Services;

/// <summary>
/// Service for sending real-time push notifications via SignalR
/// </summary>
public class PushNotificationService : IPushNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly ApiDbContext _context;

    public PushNotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<PushNotificationService> logger,
        ApiDbContext context)
    {
        _hubContext = hubContext;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Send notification to a specific user
    /// </summary>
    /// <param name="userId">Target user ID</param>
    /// <param name="notification">Notification data</param>
    /// <returns>Success status</returns>
    public async Task<bool> SendNotificationToUserAsync(string userId, PushNotificationDto notification)
    {
        try
        {
            _logger.LogInformation("Sending notification to user {UserId}: {Title}", userId, notification.Title);

            // Parse user ID
            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogError("Invalid user ID format: {UserId}", userId);
                return false;
            }

            // Save notification to database
            var dbNotification = new PushNotification
            {
                Id = Guid.NewGuid(),
                UserId = userGuid,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Priority = notification.Priority,
                Data = notification.Data != null ? JsonSerializer.Serialize(notification.Data) : null,
                ActionUrl = notification.ActionUrl,
                IconUrl = notification.IconUrl,
                RequiresAction = notification.RequiresAction,
                ExpiresAt = notification.ExpiresAt,
                CreatedAt = DateTime.UtcNow
            };

            _context.PushNotifications.Add(dbNotification);
            await _context.SaveChangesAsync();

            // Update the notification DTO with the database ID
            notification.Id = dbNotification.Id.ToString();

            // Try to send real-time notification
            bool deliveredRealTime = false;
            try
            {
                await _hubContext.Clients.Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", notification);
                deliveredRealTime = true;
            }
            catch (Exception signalrEx)
            {
                _logger.LogWarning(signalrEx, "Failed to send real-time notification to user {UserId}, but saved to database", userId);
            }

            // Update delivery status in database
            dbNotification.IsDelivered = deliveredRealTime;
            dbNotification.DeliveredAt = deliveredRealTime ? DateTime.UtcNow : null;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification saved to database for user {UserId}. Real-time delivery: {Delivered}", userId, deliveredRealTime);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Send notification to multiple users
    /// </summary>
    /// <param name="userIds">List of target user IDs</param>
    /// <param name="notification">Notification data</param>
    /// <returns>Success status</returns>
    public async Task<bool> SendNotificationToUsersAsync(IEnumerable<string> userIds, PushNotificationDto notification)
    {
        try
        {
            var userIdList = userIds.ToList();
            _logger.LogInformation("Sending notification to {UserCount} users: {Title}", userIdList.Count, notification.Title);

            var groups = userIdList.Select(userId => $"user_{userId}").ToList();
            await _hubContext.Clients.Groups(groups)
                .SendAsync("ReceiveNotification", notification);

            _logger.LogInformation("Notification sent successfully to {UserCount} users", userIdList.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to multiple users");
            return false;
        }
    }

    /// <summary>
    /// Send notification to all users with a specific role
    /// </summary>
    /// <param name="role">Target role</param>
    /// <param name="notification">Notification data</param>
    /// <returns>Success status</returns>
    public async Task<bool> SendNotificationToRoleAsync(string role, PushNotificationDto notification)
    {
        try
        {
            _logger.LogInformation("Sending notification to role {Role}: {Title}", role, notification.Title);

            await _hubContext.Clients.Group($"role_{role}")
                .SendAsync("ReceiveNotification", notification);

            _logger.LogInformation("Notification sent successfully to role {Role}", role);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to role {Role}", role);
            return false;
        }
    }

    /// <summary>
    /// Send notification to a specific group
    /// </summary>
    /// <param name="groupName">Target group name</param>
    /// <param name="notification">Notification data</param>
    /// <returns>Success status</returns>
    public async Task<bool> SendNotificationToGroupAsync(string groupName, PushNotificationDto notification)
    {
        try
        {
            _logger.LogInformation("Sending notification to group {GroupName}: {Title}", groupName, notification.Title);

            await _hubContext.Clients.Group(groupName)
                .SendAsync("ReceiveNotification", notification);

            _logger.LogInformation("Notification sent successfully to group {GroupName}", groupName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to group {GroupName}", groupName);
            return false;
        }
    }

    /// <summary>
    /// Send notification to all connected users (broadcast)
    /// </summary>
    /// <param name="notification">Notification data</param>
    /// <returns>Success status</returns>
    public async Task<bool> SendBroadcastNotificationAsync(PushNotificationDto notification)
    {
        try
        {
            _logger.LogInformation("Broadcasting notification: {Title}", notification.Title);

            await _hubContext.Clients.All
                .SendAsync("ReceiveNotification", notification);

            _logger.LogInformation("Notification broadcast successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast notification");
            return false;
        }
    }

    /// <summary>
    /// Get count of users currently online
    /// Note: This is a simplified implementation. In production, you might want to 
    /// maintain a separate service to track online users in a database or cache.
    /// </summary>
    /// <returns>Number of connected users</returns>
    public async Task<int> GetOnlineUsersCountAsync()
    {
        try
        {
            // This is a simplified approach. In production, you'd want to track this properly
            // You might use a separate service with Redis or database to track online users
            _logger.LogInformation("Getting online users count (simplified implementation)");
            
            // For now, return -1 to indicate this needs proper implementation
            return await Task.FromResult(-1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get online users count");
            return 0;
        }
    }

    /// <summary>
    /// Check if a specific user is currently online
    /// Note: This is a simplified implementation. In production, you'd want to 
    /// maintain proper user connection tracking.
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>True if user is online, false otherwise</returns>
    public async Task<bool> IsUserOnlineAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Checking if user {UserId} is online (simplified implementation)", userId);
            
            // This is a simplified approach. In production, you'd want to track this properly
            // You might use a separate service with Redis or database to track online users
            
            // For now, return false to indicate this needs proper implementation
            return await Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if user {UserId} is online", userId);
            return false;
        }
    }

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
    public async Task<bool> SendTypedNotificationAsync(
        string userId,
        Models.Enums.NotificationType type,
        string title,
        string message,
        Dictionary<string, object>? data = null,
        NotificationPriority priority = NotificationPriority.Normal)
    {
        var notification = new PushNotificationDto
        {
            Title = title,
            Message = message,
            Type = type,
            Priority = priority,
            Data = data,
            IconUrl = GetIconUrlForNotificationType(type)
        };

        return await SendNotificationToUserAsync(userId, notification);
    }

    /// <summary>
    /// Get appropriate icon URL based on notification type
    /// </summary>
    /// <param name="type">Notification type</param>
    /// <returns>Icon URL</returns>
    private static string GetIconUrlForNotificationType(Models.Enums.NotificationType type)
    {
        return type switch
        {
            Models.Enums.NotificationType.Messages => "/icons/message.png",
            Models.Enums.NotificationType.BookingUpdates => "/icons/booking.png",
            Models.Enums.NotificationType.PaymentUpdates => "/icons/payment.png",
            Models.Enums.NotificationType.PromotionsAndMarketing => "/icons/promotion.png",
            Models.Enums.NotificationType.PlatformUpdates => "/icons/platform.png",
            Models.Enums.NotificationType.System => "/icons/system.png",
            _ => "/icons/default.png"
        };
    }

    /// <summary>
    /// Get paginated notifications for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Pagination and filter parameters</param>
    /// <returns>Paginated notifications</returns>
    public async Task<PaginatedNotificationsDto> GetUserNotificationsAsync(string userId, GetNotificationsRequest request)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return new PaginatedNotificationsDto();
            }

            var query = _context.PushNotifications
                .Where(n => n.UserId == userGuid);

            // Apply filters
            if (request.Type.HasValue)
            {
                query = query.Where(n => n.Type == request.Type.Value);
            }

            if (request.Priority.HasValue)
            {
                query = query.Where(n => n.Priority == request.Priority.Value);
            }

            if (request.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == request.IsRead.Value);
            }

            if (request.IsDelivered.HasValue)
            {
                query = query.Where(n => n.IsDelivered == request.IsDelivered.Value);
            }

            if (!request.IncludeExpired)
            {
                query = query.Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow);
            }

            // Apply sorting
            query = request.SortOrder switch
            {
                "oldest" => query.OrderBy(n => n.CreatedAt),
                "priority" => query.OrderByDescending(n => n.Priority).ThenByDescending(n => n.CreatedAt),
                _ => query.OrderByDescending(n => n.CreatedAt) // "newest"
            };

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var notifications = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // Get statistics
            var stats = await GetUserNotificationStatsAsync(userId);

            // Convert to DTOs
            var notificationDtos = notifications.Select(n => new PushNotificationResponseDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                TypeName = n.Type.ToString(),
                Priority = n.Priority,
                PriorityName = n.Priority.ToString(),
                Data = !string.IsNullOrEmpty(n.Data) ? JsonSerializer.Deserialize<Dictionary<string, object>>(n.Data) : null,
                ActionUrl = n.ActionUrl,
                IconUrl = n.IconUrl,
                RequiresAction = n.RequiresAction,
                IsRead = n.IsRead,
                IsDelivered = n.IsDelivered,
                ExpiresAt = n.ExpiresAt,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt,
                DeliveredAt = n.DeliveredAt
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return new PaginatedNotificationsDto
            {
                Notifications = notificationDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                HasNextPage = request.Page < totalPages,
                HasPreviousPage = request.Page > 1,
                Stats = stats
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notifications for user {UserId}", userId);
            return new PaginatedNotificationsDto();
        }
    }

    /// <summary>
    /// Get notification statistics for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Notification statistics</returns>
    public async Task<NotificationStatsDto> GetUserNotificationStatsAsync(string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return new NotificationStatsDto();
            }

            var notifications = await _context.PushNotifications
                .Where(n => n.UserId == userGuid)
                .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            var stats = new NotificationStatsDto
            {
                TotalCount = notifications.Count,
                UnreadCount = notifications.Count(n => !n.IsRead),
                ReadCount = notifications.Count(n => n.IsRead),
                DeliveredCount = notifications.Count(n => n.IsDelivered),
                UndeliveredCount = notifications.Count(n => !n.IsDelivered)
            };

            // Count by type
            stats.CountByType = notifications
                .GroupBy(n => n.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Count by priority
            stats.CountByPriority = notifications
                .GroupBy(n => n.Priority.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification stats for user {UserId}", userId);
            return new NotificationStatsDto();
        }
    }

    /// <summary>
    /// Mark notifications as read
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="notificationIds">List of notification IDs to mark as read</param>
    /// <returns>Number of notifications marked as read</returns>
    public async Task<int> MarkNotificationsAsReadAsync(string userId, List<Guid> notificationIds)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return 0;
            }

            var notifications = await _context.PushNotifications
                .Where(n => n.UserId == userGuid && notificationIds.Contains(n.Id) && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", notifications.Count, userId);
            return notifications.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notifications as read for user {UserId}", userId);
            return 0;
        }
    }

    /// <summary>
    /// Clear notifications based on criteria
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Clear criteria</param>
    /// <returns>Number of notifications cleared</returns>
    public async Task<int> ClearNotificationsAsync(string userId, ClearNotificationsRequest request)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return 0;
            }

            var query = _context.PushNotifications.Where(n => n.UserId == userGuid);

            if (request.ClearAll)
            {
                // Clear all notifications
            }
            else if (request.ClearReadOnly)
            {
                query = query.Where(n => n.IsRead);
            }
            else if (request.NotificationIds?.Any() == true)
            {
                query = query.Where(n => request.NotificationIds.Contains(n.Id));
            }
            else
            {
                // Apply other filters
                if (request.OlderThanDays.HasValue)
                {
                    var cutoffDate = DateTime.UtcNow.AddDays(-request.OlderThanDays.Value);
                    query = query.Where(n => n.CreatedAt < cutoffDate);
                }

                if (request.Type.HasValue)
                {
                    query = query.Where(n => n.Type == request.Type.Value);
                }
            }

            var notificationsToDelete = await query.ToListAsync();
            
            _context.PushNotifications.RemoveRange(notificationsToDelete);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleared {Count} notifications for user {UserId}", notificationsToDelete.Count, userId);
            return notificationsToDelete.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear notifications for user {UserId}", userId);
            return 0;
        }
    }
}
