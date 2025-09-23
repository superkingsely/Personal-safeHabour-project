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
    /// <param name="baseUrl">Base URL for constructing full profile picture URLs (optional)</param>
    /// <returns>ServiceWorkerDto</returns>
    public static ServiceWorkerDto ToDtoWithUserDetails(ServiceWorkerUser serviceWorkerUser, string? baseUrl = null)
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
            ProfilePicturePath = CombineUrlWithBasePath(serviceWorkerUser.User.ProfilePicturePath, baseUrl),
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

    /// <summary>
    /// Maps ServiceWorkerUser entity to ServiceWorkerSearchResultDto with distance calculation
    /// </summary>
    /// <param name="serviceWorker">The ServiceWorkerUser entity with User navigation property loaded</param>
    /// <param name="distance">The calculated distance in kilometers</param>
    /// <param name="baseUrl">Base URL for constructing full profile picture URLs (optional)</param>
    /// <returns>ServiceWorkerSearchResultDto</returns>
    public static ServiceWorkerSearchResultDto ToSearchResultDto(ServiceWorkerUser serviceWorker, double distance, string? baseUrl = null)
    {
        return new ServiceWorkerSearchResultDto
        {
            Id = serviceWorker.Id,
            UserId = serviceWorker.UserId.ToString(),
            FirstName = serviceWorker.User.FirstName,
            LastName = serviceWorker.User.LastName,
            Email = serviceWorker.User.Email ?? string.Empty,
            PhoneNumber = serviceWorker.User.PhoneNumber,
            DateOfBirth = serviceWorker.User.DateOfBirth,
            Gender = serviceWorker.User.Gender,
            Bio = serviceWorker.User.Bio,
            ProfilePicturePath = CombineUrlWithBasePath(serviceWorker.User.ProfilePicturePath, baseUrl),
            StreetAddress = serviceWorker.User.StreetAddress,
            City = serviceWorker.User.City,
            Country = serviceWorker.User.Country,
            PostalCode = serviceWorker.User.PostalCode,
            Latitude = serviceWorker.User.Latitude ?? serviceWorker.Latitude,
            Longitude = serviceWorker.User.Longitude ?? serviceWorker.Longitude,
            Services = serviceWorker.Services,
            Languages = serviceWorker.Languages,
            HourlyRate = serviceWorker.HourlyRate,
            DistanceKm = distance == 9999 ? null : Math.Round(distance, 2),
            AverageRating = null, // TODO: Calculate when review system is implemented
            ReviewCount = 0, // TODO: Count when review system is implemented
            IsAvailable = true, // TODO: Implement availability system
            IsVerified = serviceWorker.User.EmailConfirmed,
            JoinedDate = serviceWorker.CreatedAt,
            LastActiveDate = serviceWorker.User.UpdatedAt
        };
    }

    /// <summary>
    /// Combines a relative file path with a base URL to create a complete URL
    /// </summary>
    /// <param name="relativePath">The relative file path (can be null or empty)</param>
    /// <param name="baseUrl">The base URL (can be null or empty)</param>
    /// <returns>Complete URL if both parameters are provided, otherwise the relative path or null</returns>
    private static string? CombineUrlWithBasePath(string? relativePath, string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return relativePath;

        if (string.IsNullOrWhiteSpace(baseUrl))
            return relativePath;

        // Remove trailing slash from baseUrl and leading slash from relativePath to avoid double slashes
        var cleanBaseUrl = baseUrl.TrimEnd('/');
        var cleanRelativePath = relativePath.TrimStart('/');

        return $"{cleanBaseUrl}/{cleanRelativePath}";
    }
}
