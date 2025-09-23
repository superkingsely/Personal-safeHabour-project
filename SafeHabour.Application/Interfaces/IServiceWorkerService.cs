using SafeHabour.Models.Response;
using SafeHabour.Models.Requests;

namespace SafeHabour.Application.Interfaces;

public interface IServiceWorkerService
{
    /// <summary>
    /// Searches for service workers with pagination and location-based filtering
    /// </summary>
    /// <param name="searchRequest">Search parameters</param>
    /// <param name="currentUserId">Current user ID for location fallback (optional)</param>
    /// <returns>ServiceResult containing paginated search results</returns>
    Task<ServiceResult<PaginatedResponse<ServiceWorkerSearchResultDto>>> SearchServiceWorkersAsync(
        SearchServiceWorkersRequest searchRequest, 
        Guid? currentUserId = null);

    /// <summary>
    /// Gets service worker details by user ID with business logic validation
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>ServiceResult containing ServiceWorkerDto if found and accessible</returns>
    Task<ServiceResult<ServiceWorkerDto>> GetServiceWorkerByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets service worker details by service worker ID with business logic validation
    /// </summary>
    /// <param name="serviceWorkerId">The service worker ID</param>
    /// <returns>ServiceResult containing ServiceWorkerDto if found and accessible</returns>
    Task<ServiceResult<ServiceWorkerDto>> GetServiceWorkerByIdAsync(int serviceWorkerId);

    /// <summary>
    /// Gets service worker profile completion status
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>ServiceResult containing profile completion information</returns>
    Task<ServiceResult<ServiceWorkerProfileStatus>> GetProfileCompletionStatusAsync(Guid userId);

    /// <summary>
    /// Updates a service worker profile with file upload handling
    /// </summary>
    /// <param name="request">The update service worker request</param>
    /// <returns>ServiceResult containing updated ServiceWorkerDto if successful</returns>
    Task<ServiceResult<ServiceWorkerDto>> UpdateServiceWorkerAsync(UpdateServiceWorkerRequest request);
}
