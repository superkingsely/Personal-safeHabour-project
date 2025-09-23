using SafeHabour.Models.Enums;

namespace SafeHabour.Models.Response;

public class UserNotificationSettingDto
{
    public Guid Id { get; set; }
    public NotificationType NotificationType { get; set; }
    public string NotificationTypeName { get; set; } = string.Empty;
    public bool EmailNotificationEnabled { get; set; }
    public bool InAppNotificationEnabled { get; set; }
}
