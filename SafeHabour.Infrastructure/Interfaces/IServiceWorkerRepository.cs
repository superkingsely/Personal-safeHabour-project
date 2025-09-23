using SafeHabour.Models.Response;
using SafeHabour.Models.Requests;

namespace SafeHabour.Infrastructure.Interfaces;

public interface IServiceWorkerRepository
{
    /// <summary>
    /// Gets service worker details by user ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="baseUrl">Base URL for constructing complete profile picture URLs (optional)</param>
    /// <returns>ServiceWorkerDto if found, null otherwise</returns>
    Task<ServiceWorkerDto?> GetServiceWorkerByUserIdAsync(Guid userId, string? baseUrl = null);

    /// <summary>
    /// Gets service worker details by service worker ID
    /// </summary>
    /// <param name="serviceWorkerId">The service worker ID</param>
    /// <param name="baseUrl">Base URL for constructing complete profile picture URLs (optional)</param>
    /// <returns>ServiceWorkerDto if found, null otherwise</returns>
    Task<ServiceWorkerDto?> GetServiceWorkerByIdAsync(int serviceWorkerId, string? baseUrl = null);

    /// <summary>
    /// Updates a service worker
    /// </summary>
    /// <param name="request">The update service worker request</param>
    /// <param name="profilePicturePath">The relative path to the uploaded profile picture (optional)</param>
    /// <param name="baseUrl">Base URL for constructing complete profile picture URLs (optional)</param>
    /// <returns>Updated ServiceWorkerDto if successful, null otherwise</returns>
    Task<ServiceWorkerDto?> UpdateServiceWorkerAsync(UpdateServiceWorkerRequest request, string? profilePicturePath = null, string? baseUrl = null);

    /// <summary>
    /// Searches for service workers with pagination and location-based filtering
    /// </summary>
    /// <param name="searchRequest">Search parameters</param>
    /// <param name="currentUserId">Current user ID for fallback location</param>
    /// <param name="baseUrl">Base URL for constructing complete profile picture URLs (optional)</param>
    /// <returns>Paginated list of service workers</returns>
    Task<PaginatedResponse<ServiceWorkerSearchResultDto>> SearchServiceWorkersAsync(SearchServiceWorkersRequest searchRequest, Guid? currentUserId = null, string? baseUrl = null);
}
