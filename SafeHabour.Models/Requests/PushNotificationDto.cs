using System;
using SafeHabour.Models.Enums;

namespace SafeHabour.Models.Requests;

/// <summary>
/// DTO for push notifications
/// </summary>
public class PushNotificationDto
{
    /// <summary>
    /// Unique notification ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Notification title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification message/body
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Type of notification
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Priority level of the notification
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// Additional data for the notification (optional)
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }

    /// <summary>
    /// URL or action to take when notification is clicked (optional)
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Icon URL for the notification (optional)
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Timestamp when notification was created
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the notification requires user action
    /// </summary>
    public bool RequiresAction { get; set; } = false;

    /// <summary>
    /// Expiry time for the notification (optional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
