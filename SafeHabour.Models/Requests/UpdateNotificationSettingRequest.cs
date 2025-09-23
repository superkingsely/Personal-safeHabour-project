using SafeHabour.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Models.Requests;

public class UpdateNotificationSettingRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public NotificationType NotificationType { get; set; }
    
    public bool EmailNotificationEnabled { get; set; }
    
    public bool InAppNotificationEnabled { get; set; }
}
