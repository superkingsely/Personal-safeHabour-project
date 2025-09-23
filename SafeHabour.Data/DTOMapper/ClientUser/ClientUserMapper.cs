using SafeHabour.Data.Entities;
using SafeHabour.Models.Response;

namespace SafeHabour.Data.DTOMapper.ClientUser;

public static class ClientUserMapper
{
    /// <summary>
    /// Maps ClientUser entity to ClientUserDto (basic info only)
    /// </summary>
    /// <param name="clientUser">The ClientUser entity</param>
    /// <returns>ClientUserDto</returns>
    public static ClientUserDto ToDto(SafeHabour.Data.Entities.ClientUser clientUser)
    {
        return new ClientUserDto
        {
            Id = clientUser.Id,
            UserId = clientUser.UserId.ToString(),
            ClientType = clientUser.ClientType,
            FirstName = string.Empty,
            LastName = string.Empty,
            Email = string.Empty,
            // User details will be null - use ToDtoWithUserDetails for complete mapping
        };
    }

    /// <summary>
    /// Maps ClientUser entity with User details to ClientUserDto (complete mapping)
    /// </summary>
    /// <param name="clientUser">The ClientUser entity with User navigation property loaded</param>
    /// <returns>ClientUserDto</returns>
    public static ClientUserDto ToDtoWithUserDetails(SafeHabour.Data.Entities.ClientUser clientUser)
    {
        if (clientUser.User == null)
            return ToDto(clientUser);

        return new ClientUserDto
        {
            Id = clientUser.Id,
            UserId = clientUser.UserId.ToString(),
            ClientType = clientUser.ClientType,
            FirstName = clientUser.User.FirstName,
            LastName = clientUser.User.LastName,
            Email = clientUser.User.Email ?? string.Empty,
            PhoneNumber = clientUser.User.PhoneNumber,
            DateOfBirth = clientUser.User.DateOfBirth,
            Gender = clientUser.User.Gender,
            Bio = clientUser.User.Bio,
            ProfilePicturePath = clientUser.User.ProfilePicturePath,
            StreetAddress = clientUser.User.StreetAddress,
            City = clientUser.User.City,
            Country = clientUser.User.Country,
            PostalCode = clientUser.User.PostalCode,
            Latitude = clientUser.User.Latitude,
            Longitude = clientUser.User.Longitude
        };
    }

    /// <summary>
    /// Maps ClientUserDto to ClientUser entity (for updates/creation)
    /// </summary>
    /// <param name="dto">The ClientUserDto</param>
    /// <returns>ClientUser entity</returns>
    public static SafeHabour.Data.Entities.ClientUser ToEntity(ClientUserDto dto)
    {
        return new SafeHabour.Data.Entities.ClientUser
        {
            Id = dto.Id,
            ClientType = dto.ClientType,
            UserId = Guid.Parse(dto.UserId),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates an existing ClientUser entity with values from DTO
    /// </summary>
    /// <param name="entity">The existing ClientUser entity</param>
    /// <param name="dto">The ClientUserDto with updated values</param>
    public static void UpdateEntityFromDto(SafeHabour.Data.Entities.ClientUser entity, ClientUserDto dto)
    {
        entity.ClientType = dto.ClientType;
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Note: User details are updated separately in the User entity
        // Id and UserId should not be updated
    }
}
