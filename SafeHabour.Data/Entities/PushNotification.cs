using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SafeHabour.Models.Enums;

namespace SafeHabour.Data.Entities;

/// <summary>
/// Entity for persisting push notifications in the database
/// </summary>
public class PushNotification
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the user who should receive this notification
    /// </summary>
    [Required]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    /// <summary>
    /// Notification title
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification message/body
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Type of notification
    /// </summary>
    [Required]
    public NotificationType Type { get; set; }

    /// <summary>
    /// Priority level of the notification
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// Additional data for the notification (stored as JSON)
    /// </summary>
    [StringLength(2000)]
    public string? Data { get; set; }

    /// <summary>
    /// URL or action to take when notification is clicked (optional)
    /// </summary>
    [StringLength(500)]
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Icon URL for the notification (optional)
    /// </summary>
    [StringLength(500)]
    public string? IconUrl { get; set; }

    /// <summary>
    /// Whether the notification requires user action
    /// </summary>
    public bool RequiresAction { get; set; } = false;

    /// <summary>
    /// Whether the notification has been read by the user
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Whether the notification was successfully delivered via push
    /// </summary>
    public bool IsDelivered { get; set; } = false;

    /// <summary>
    /// Expiry time for the notification (optional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Timestamp when notification was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when notification was read (if read)
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Timestamp when notification was delivered (if delivered)
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
