using SafeHabour.Data.Entities;
using SafeHabour.Models.Response;
using SafeHabour.Models.Enums;

namespace SafeHabour.Data.DTOMapper.NotificationSetting;

public static class NotificationSettingMapper
{
    /// <summary>
    /// Maps UserNotificationSetting entity to UserNotificationSettingDto
    /// </summary>
    /// <param name="setting">The UserNotificationSetting entity</param>
    /// <returns>UserNotificationSettingDto</returns>
    public static UserNotificationSettingDto ToDto(UserNotificationSetting setting)
    {
        return new UserNotificationSettingDto
        {
            Id = setting.Id,
            NotificationType = setting.NotificationType,
            NotificationTypeName = setting.NotificationType.ToString(),
            EmailNotificationEnabled = setting.EmailNotificationEnabled,
            InAppNotificationEnabled = setting.InAppNotificationEnabled
        };
    }

    /// <summary>
    /// Maps a collection of UserNotificationSetting entities to DTOs
    /// </summary>
    /// <param name="settings">Collection of UserNotificationSetting entities</param>
    /// <returns>List of UserNotificationSettingDto</returns>
    public static List<UserNotificationSettingDto> ToDto(ICollection<UserNotificationSetting> settings)
    {
        return settings.Select(ToDto).ToList();
    }

    /// <summary>
    /// Updates an existing UserNotificationSetting entity with values from a DTO
    /// </summary>
    /// <param name="entity">The existing UserNotificationSetting entity</param>
    /// <param name="dto">The UserNotificationSettingDto with updated values</param>
    public static void UpdateEntityFromDto(UserNotificationSetting entity, UserNotificationSettingDto dto)
    {
        entity.EmailNotificationEnabled = dto.EmailNotificationEnabled;
        entity.InAppNotificationEnabled = dto.InAppNotificationEnabled;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
