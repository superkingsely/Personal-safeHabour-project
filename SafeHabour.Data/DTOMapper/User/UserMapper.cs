using Microsoft.AspNetCore.Identity;
using SafeHabour.Data.Entities;
using SafeHabour.Models.Response;
using SafeHabour.Data.DTOMapper.NotificationSetting;

namespace SafeHabour.Data.DTOMapper.User;

public static class UserMapper
{
    /// <summary>
    /// Maps User entity to UserDto
    /// </summary>
    /// <param name="user">The User entity</param>
    /// <param name="userManager">UserManager instance for getting roles</param>
    /// <returns>UserDto</returns>
    public static async Task<UserDto> ToDtoAsync(SafeHabour.Data.Entities.User user, UserManager<SafeHabour.Data.Entities.User> userManager)
    {
        var roles = await userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = $"{user.FirstName} {user.LastName}",
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Bio = user.Bio,
            ProfilePicturePath = user.ProfilePicturePath,
            StreetAddress = user.StreetAddress,
            City = user.City,
            Country = user.Country,
            PostalCode = user.PostalCode,
            Latitude = user.Latitude,
            Longitude = user.Longitude,
            IsProfileComplete = user.IsProfileComplete,
            IsVerified = user.IsVerified,
            EmailConfirmed = user.EmailConfirmed,
            IsActive = user.IsActive,
            IsStripeConnectEnabled = user.IsStripeConnectEnabled,
            HasIdentificationDocument = !string.IsNullOrEmpty(user.UserIdentificationDocumentPath),
            HasLocationDocument = !string.IsNullOrEmpty(user.UserLocationDocumentPath),
            IsTwoFactorAuthenticationEnabled = user.IsTwoFactorAuthenticationEnabled,
            NotificationSettings = NotificationSettingMapper.ToDto(user.NotificationSettings),
            Roles = roles.ToList()
        };
    }

    /// <summary>
    /// Maps User entity to UserDto with pre-fetched roles
    /// </summary>
    /// <param name="user">The User entity</param>
    /// <param name="roles">Pre-fetched user roles</param>
    /// <returns>UserDto</returns>
    public static UserDto ToDto(SafeHabour.Data.Entities.User user, IList<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = $"{user.FirstName} {user.LastName}",
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Bio = user.Bio,
            ProfilePicturePath = user.ProfilePicturePath,
            StreetAddress = user.StreetAddress,
            City = user.City,
            Country = user.Country,
            PostalCode = user.PostalCode,
            Latitude = user.Latitude,
            Longitude = user.Longitude,
            IsProfileComplete = user.IsProfileComplete,
            IsVerified = user.IsVerified,
            EmailConfirmed = user.EmailConfirmed,
            IsActive = user.IsActive,
            IsStripeConnectEnabled = user.IsStripeConnectEnabled,
            HasIdentificationDocument = !string.IsNullOrEmpty(user.UserIdentificationDocumentPath),
            HasLocationDocument = !string.IsNullOrEmpty(user.UserLocationDocumentPath),
            IsTwoFactorAuthenticationEnabled = user.IsTwoFactorAuthenticationEnabled,
            NotificationSettings = NotificationSettingMapper.ToDto(user.NotificationSettings),
            Roles = roles.ToList()
        };
    }

    /// <summary>
    /// Updates an existing User entity with values from UserDto
    /// </summary>
    /// <param name="entity">The existing User entity</param>
    /// <param name="dto">The UserDto with updated values</param>
    public static void UpdateEntityFromDto(SafeHabour.Data.Entities.User entity, UserDto dto)
    {
        entity.FirstName = dto.FirstName;
        entity.LastName = dto.LastName;
        entity.PhoneNumber = dto.PhoneNumber;
        entity.DateOfBirth = dto.DateOfBirth;
        entity.Gender = dto.Gender;
        entity.Bio = dto.Bio;
        entity.StreetAddress = dto.StreetAddress;
        entity.City = dto.City;
        entity.Country = dto.Country;
        entity.PostalCode = dto.PostalCode;
        entity.Latitude = dto.Latitude;
        entity.Longitude = dto.Longitude;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Note: Email, Id, roles, and verification fields should be handled separately
        // IsProfileComplete should be managed by business logic, not direct DTO mapping
        // ProfilePicturePath should be handled separately through file upload logic
    }
}
