using System;
using SafeHabour.Models.Enums;

namespace SafeHabour.Models.Requests;

public class CustomNotificationRequest
{
     public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.System;
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public Dictionary<string, object>? Data { get; set; }
    public string? ActionUrl { get; set; }
    public bool RequiresAction { get; set; } = false;
    
    // Target specification
    public List<string>? UserIds { get; set; }
    public string? Role { get; set; }
    public bool BroadcastToAll { get; set; } = false;
}
