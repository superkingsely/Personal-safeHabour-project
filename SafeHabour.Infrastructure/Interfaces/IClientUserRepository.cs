using SafeHabour.Data.Entities;
using SafeHabour.Models.Response;
using SafeHabour.Models.Requests;

namespace SafeHabour.Infrastructure.Interfaces;

public interface IClientUserRepository
{
    /// <summary>
    /// Gets client user details by user ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>ClientUserDto if found, null otherwise</returns>
    Task<ClientUserDto?> GetClientUserByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets client user details by client user ID
    /// </summary>
    /// <param name="clientUserId">The client user ID</param>
    /// <returns>ClientUserDto if found, null otherwise</returns>
    Task<ClientUserDto?> GetClientUserByIdAsync(int clientUserId);

    /// <summary>
    /// Updates a client user
    /// </summary>
    /// <param name="request">The update client user request</param>
    /// <param name="profilePicturePath">The relative path to the uploaded profile picture (optional)</param>
    /// <returns>Updated ClientUserDto if successful, null otherwise</returns>
    Task<ClientUserDto?> UpdateClientUserAsync(UpdateClientUserRequest request, string? profilePicturePath = null);
}
