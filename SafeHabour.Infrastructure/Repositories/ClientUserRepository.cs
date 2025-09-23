using Microsoft.EntityFrameworkCore;
using SafeHabour.Data;
using SafeHabour.Data.Data;
using SafeHabour.Data.Entities;
using SafeHabour.Infrastructure.Interfaces;
using SafeHabour.Models.Response;
using SafeHabour.Models.Requests;
using SafeHabour.Data.DTOMapper.ClientUser;

namespace SafeHabour.Infrastructure.Repositories;

public class ClientUserRepository : IClientUserRepository
{
    private readonly ApiDbContext _context;

    public ClientUserRepository(ApiDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets client user details by user ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>ClientUserDto if found, null otherwise</returns>
    public async Task<ClientUserDto?> GetClientUserByUserIdAsync(Guid userId)
    {
        try
        {
            var clientUser = await _context.ClientUsers
                .Include(cu => cu.User)
                .FirstOrDefaultAsync(cu => cu.UserId == userId);

            if (clientUser == null)
                return null;

            return ClientUserMapper.ToDtoWithUserDetails(clientUser);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets client user details by client user ID
    /// </summary>
    /// <param name="clientUserId">The client user ID</param>
    /// <returns>ClientUserDto if found, null otherwise</returns>
    public async Task<ClientUserDto?> GetClientUserByIdAsync(int clientUserId)
    {
        try
        {
            var clientUser = await _context.ClientUsers
                .Include(cu => cu.User)
                .FirstOrDefaultAsync(cu => cu.Id == clientUserId);

            if (clientUser == null)
                return null;

            return ClientUserMapper.ToDtoWithUserDetails(clientUser);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Updates a client user
    /// </summary>
    /// <param name="request">The update client user request</param>
    /// <param name="profilePicturePath">The relative path to the uploaded profile picture (optional)</param>
    /// <returns>Updated ClientUserDto if successful, null otherwise</returns>
    public async Task<ClientUserDto?> UpdateClientUserAsync(UpdateClientUserRequest request, string? profilePicturePath = null)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out var userId))
                return null;

            var clientUser = await _context.ClientUsers
                .Include(cu => cu.User)
                .FirstOrDefaultAsync(cu => cu.UserId == userId);

            if (clientUser == null)
                return null;

            var user = clientUser.User;

            // Update user fields if provided
            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            if (request.DateOfBirth.HasValue)
                user.DateOfBirth = request.DateOfBirth.Value;

            if (!string.IsNullOrEmpty(request.Gender))
                user.Gender = request.Gender;

            if (!string.IsNullOrEmpty(request.Bio))
                user.Bio = request.Bio;

            if (!string.IsNullOrEmpty(profilePicturePath))
                user.ProfilePicturePath = profilePicturePath;

            if (!string.IsNullOrEmpty(request.StreetAddress))
                user.StreetAddress = request.StreetAddress;

            if (!string.IsNullOrEmpty(request.City))
                user.City = request.City;

            if (!string.IsNullOrEmpty(request.Country))
                user.Country = request.Country;

            if (!string.IsNullOrEmpty(request.PostalCode))
                user.PostalCode = request.PostalCode;

            // Update coordinates if provided
            if (request.Latitude.HasValue)
                user.Latitude = request.Latitude.Value;

            if (request.Longitude.HasValue)
                user.Longitude = request.Longitude.Value;

            // Update client type if provided
            if (request.ClientType.HasValue)
                clientUser.ClientType = (int)request.ClientType.Value;

            // Update timestamps
            user.UpdatedAt = DateTime.UtcNow;
            clientUser.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            _context.ClientUsers.Update(clientUser);
            await _context.SaveChangesAsync();

            return ClientUserMapper.ToDtoWithUserDetails(clientUser);
        }
        catch
        {
            return null;
        }
    }
}
