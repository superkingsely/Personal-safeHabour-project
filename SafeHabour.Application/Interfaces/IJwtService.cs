using SafeHabour.Data.Entities;

namespace SafeHabour.Application.Interfaces;

public interface IJwtService
{
    /// <summary>
    /// Generates a JWT token for the specified user
    /// </summary>
    /// <param name="user">User to generate token for</param>
    /// <param name="roles">User roles to include in token</param>
    /// <returns>Token string and expiry date</returns>
    Task<(string Token, DateTime Expiry)> GenerateJwtTokenAsync(User user, IList<string> roles);

    /// <summary>
    /// Validates a JWT token
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <returns>Validation result</returns>
    Task<bool> ValidateTokenAsync(string token);

    /// <summary>
    /// Extracts user ID from a JWT token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>User ID if valid, null otherwise</returns>
    Task<Guid?> GetUserIdFromTokenAsync(string token);
}
