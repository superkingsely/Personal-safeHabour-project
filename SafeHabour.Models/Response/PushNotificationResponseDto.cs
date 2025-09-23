using SafeHabour.Models.Enums;

namespace SafeHabour.Models.Response;

/// <summary>
/// DTO for push notification responses
/// </summary>
public class PushNotificationResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public NotificationPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public Dictionary<string, object>? Data { get; set; }
    public string? ActionUrl { get; set; }
    public string? IconUrl { get; set; }
    public bool RequiresAction { get; set; }
    public bool IsRead { get; set; }
    public bool IsDelivered { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
