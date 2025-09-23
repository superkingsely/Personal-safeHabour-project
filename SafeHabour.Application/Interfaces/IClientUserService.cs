using SafeHabour.Models.Response;
using SafeHabour.Models.Requests;

namespace SafeHabour.Application.Interfaces;

public interface IClientUserService
{
    /// <summary>
    /// Gets client user details by user ID with business logic validation
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>ServiceResult containing ClientUserDto if found and accessible</returns>
    Task<ServiceResult<ClientUserDto>> GetClientUserByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets client user details by client user ID with business logic validation
    /// </summary>
    /// <param name="clientUserId">The client user ID</param>
    /// <returns>ServiceResult containing ClientUserDto if found and accessible</returns>
    Task<ServiceResult<ClientUserDto>> GetClientUserByIdAsync(int clientUserId);

    /// <summary>
    /// Gets client user profile completion status
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>ServiceResult containing profile completion information</returns>
    Task<ServiceResult<ClientUserProfileStatus>> GetProfileCompletionStatusAsync(Guid userId);

    /// <summary>
    /// Updates a client user profile with file upload handling
    /// </summary>
    /// <param name="request">The update client user request</param>
    /// <returns>ServiceResult containing updated ClientUserDto if successful</returns>
    Task<ServiceResult<ClientUserDto>> UpdateClientUserAsync(UpdateClientUserRequest request);
}

