using SafeHabour.Data.Entities;
using SafeHabour.Models.Response;

namespace SafeHabour.Data.DTOMapper.ServiceWorker;

public static class ServiceWorkerMapper
{
    /// <summary>
    /// Maps ServiceWorkerUser entity to ServiceWorkerDto (basic info only)
    /// </summary>
    /// <param name="serviceWorkerUser">The ServiceWorkerUser entity</param>
    /// <returns>ServiceWorkerDto</returns>
    public static ServiceWorkerDto ToDto(ServiceWorkerUser serviceWorkerUser)
    {
        return new ServiceWorkerDto
        {
            Id = serviceWorkerUser.Id,
            UserId = serviceWorkerUser.UserId.ToString(),
            FirstName = string.Empty,
            LastName = string.Empty,
            Email = string.Empty,
            Services = serviceWorkerUser.Services,
            Languages = serviceWorkerUser.Languages,
            HourlyRate = serviceWorkerUser.HourlyRate,
            // User details will be null - use ToDtoWithUserDetails for complete mapping
        };
    }

    /// <summary>
    /// Maps ServiceWorkerUser entity with User details to ServiceWorkerDto (complete mapping)
    /// </summary>
    /// <param name="serviceWorkerUser">The ServiceWorkerUser entity with User navigation property loaded</param>
    /// <returns>ServiceWorkerDto</returns>
    public static ServiceWorkerDto ToDtoWithUserDetails(ServiceWorkerUser serviceWorkerUser)
    {
        if (serviceWorkerUser.User == null)
            return ToDto(serviceWorkerUser);

        return new ServiceWorkerDto
        {
            Id = serviceWorkerUser.Id,
            UserId = serviceWorkerUser.UserId.ToString(),
            FirstName = serviceWorkerUser.User.FirstName,
            LastName = serviceWorkerUser.User.LastName,
            Email = serviceWorkerUser.User.Email ?? string.Empty,
            PhoneNumber = serviceWorkerUser.User.PhoneNumber,
            DateOfBirth = serviceWorkerUser.User.DateOfBirth,
            Gender = serviceWorkerUser.User.Gender,
            Bio = serviceWorkerUser.User.Bio,
            ProfilePicturePath = serviceWorkerUser.User.ProfilePicturePath,
            StreetAddress = serviceWorkerUser.User.StreetAddress,
            City = serviceWorkerUser.User.City,
            Country = serviceWorkerUser.User.Country,
            PostalCode = serviceWorkerUser.User.PostalCode,
            Latitude = serviceWorkerUser.User.Latitude,
            Longitude = serviceWorkerUser.User.Longitude,
            Services = serviceWorkerUser.Services,
            Languages = serviceWorkerUser.Languages,
            HourlyRate = serviceWorkerUser.HourlyRate
        };
    }

    /// <summary>
    /// Maps ServiceWorkerDto to ServiceWorkerUser entity (for updates/creation)
    /// </summary>
    /// <param name="dto">The ServiceWorkerDto</param>
    /// <returns>ServiceWorkerUser entity</returns>
    public static ServiceWorkerUser ToEntity(ServiceWorkerDto dto)
    {
        return new ServiceWorkerUser
        {
            Id = dto.Id,
            UserId = Guid.Parse(dto.UserId),
            Services = dto.Services,
            Languages = dto.Languages,
            HourlyRate = dto.HourlyRate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates an existing ServiceWorkerUser entity with values from DTO
    /// </summary>
    /// <param name="entity">The existing ServiceWorkerUser entity</param>
    /// <param name="dto">The ServiceWorkerDto with updated values</param>
    public static void UpdateEntityFromDto(ServiceWorkerUser entity, ServiceWorkerDto dto)
    {
        entity.Services = dto.Services;
        entity.Languages = dto.Languages;
        entity.HourlyRate = dto.HourlyRate;
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Note: User details are updated separately in the User entity
        // Id and UserId should not be updated
    }
}
