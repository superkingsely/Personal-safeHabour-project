using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SafeHabour.Models.Enums;

namespace SafeHabour.Data.Entities;

public class UserNotificationSetting
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [Required]
    public NotificationType NotificationType { get; set; }

    /// <summary>
    /// Whether the user wants to receive email notifications for this type
    /// </summary>
    public bool EmailNotificationEnabled { get; set; } = false;

    /// <summary>
    /// Whether the user wants to receive in-app notifications for this type
    /// </summary>
    public bool InAppNotificationEnabled { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual User User { get; set; } = null!;
}
