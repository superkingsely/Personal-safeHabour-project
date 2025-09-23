using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeHabour.Application.Interfaces;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;
using System.Security.Claims;

namespace SafeHabour.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceWorkersController : ControllerBase
{
    private readonly IServiceWorkerService _serviceWorkerService;
    private readonly ILogger<ServiceWorkersController> _logger;

    public ServiceWorkersController(
        IServiceWorkerService serviceWorkerService,
        ILogger<ServiceWorkersController> logger)
    {
        _serviceWorkerService = serviceWorkerService;
        _logger = logger;
    }

    /// <summary>
    /// Search for service workers with pagination and location-based filtering
    /// </summary>
    /// <param name="searchRequest">Search parameters</param>
    /// <returns>Paginated list of service workers</returns>
    [HttpPost("search")]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedResponse<ServiceWorkerSearchResultDto>>> SearchServiceWorkers(
        [FromBody] SearchServiceWorkersRequest searchRequest)
    {
        try
        {
            _logger.LogInformation("Searching service workers with parameters: Page={Page}, PageSize={PageSize}, SearchTerm={SearchTerm}, Latitude={Latitude}, Longitude={Longitude}, RadiusKm={RadiusKm}",
                searchRequest.Page, searchRequest.PageSize, searchRequest.SearchTerm, searchRequest.Latitude, searchRequest.Longitude, searchRequest.RadiusKm);

            // Get current user ID if authenticated (for fallback location)
            Guid? currentUserId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    currentUserId = userId;
                    _logger.LogDebug("Using authenticated user {UserId} for location fallback", userId);
                }
            }

            var result = await _serviceWorkerService.SearchServiceWorkersAsync(searchRequest, currentUserId);

            if (!result.Success)
            {
                _logger.LogWarning("Service worker search failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            _logger.LogInformation("Service worker search completed: Found {TotalCount} results, Page {Page}/{TotalPages}, UsedFallbackLocation={UsedFallback}",
                result.Data?.TotalCount, result.Data?.Page, result.Data?.TotalPages, result.Data?.UsedFallbackLocation);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching service workers");
            return StatusCode(500, new { message = "An error occurred while searching service workers" });
        }
    }

    /// <summary>
    /// Get service worker details by ID
    /// </summary>
    /// <param name="id">Service worker ID</param>
    /// <returns>Service worker details</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceWorkerDto>> GetServiceWorkerById(int id)
    {
        try
        {
            _logger.LogInformation("Getting service worker details for ID: {ServiceWorkerId}", id);

            var result = await _serviceWorkerService.GetServiceWorkerByIdAsync(id);

            if (!result.Success)
            {
                _logger.LogWarning("Service worker not found with ID: {ServiceWorkerId} - {Message}", id, result.Message);
                return NotFound(new { message = result.Message });
            }

            _logger.LogInformation("Successfully retrieved service worker: {ServiceWorkerId} - {FirstName} {LastName}",
                id, result.Data?.FirstName, result.Data?.LastName);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting service worker {ServiceWorkerId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the service worker" });
        }
    }

    /// <summary>
    /// Get service worker details by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Service worker details</returns>
    [HttpGet("by-user/{userId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceWorkerDto>> GetServiceWorkerByUserId(string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}", userId);
                return BadRequest(new { message = "Invalid user ID format" });
            }

            _logger.LogInformation("Getting service worker details for user ID: {UserId}", userGuid);

            var result = await _serviceWorkerService.GetServiceWorkerByUserIdAsync(userGuid);

            if (!result.Success)
            {
                _logger.LogWarning("Service worker not found for user ID: {UserId} - {Message}", userGuid, result.Message);
                return NotFound(new { message = result.Message });
            }

            _logger.LogInformation("Successfully retrieved service worker for user: {UserId} - {FirstName} {LastName}",
                userGuid, result.Data?.FirstName, result.Data?.LastName);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting service worker for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving the service worker" });
        }
    }

    /// <summary>
    /// Update service worker profile (requires authentication)
    /// </summary>
    /// <param name="request">Update request</param>
    /// <returns>Updated service worker details</returns>
    [HttpPut("update")]
    [Authorize]
    public async Task<ActionResult<ServiceWorkerDto>> UpdateServiceWorker([FromForm] UpdateServiceWorkerRequest request)
    {
        try
        {
            _logger.LogInformation("Updating service worker profile for user: {UserId}", request.UserId);

            var result = await _serviceWorkerService.UpdateServiceWorkerAsync(request);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to update service worker for user: {UserId} - {Message}", request.UserId, result.Message);
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            _logger.LogInformation("Successfully updated service worker profile for user: {UserId}", request.UserId);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating service worker for user {UserId}", request.UserId);
            return StatusCode(500, new { message = "An error occurred while updating the service worker profile" });
        }
    }

    /// <summary>
    /// Get profile completion status for a service worker
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Profile completion status</returns>
    [HttpGet("profile-status/{userId}")]
    [Authorize]
    public async Task<ActionResult<ServiceWorkerProfileStatus>> GetProfileCompletionStatus(string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Invalid user ID format for profile status: {UserId}", userId);
                return BadRequest(new { message = "Invalid user ID format" });
            }

            _logger.LogInformation("Getting profile completion status for user ID: {UserId}", userGuid);

            var result = await _serviceWorkerService.GetProfileCompletionStatusAsync(userGuid);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to get profile completion status for user: {UserId} - {Message}", userGuid, result.Message);
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            _logger.LogInformation("Successfully retrieved profile completion status for user: {UserId} - {CompletionPercentage}%",
                userGuid, result.Data?.CompletionPercentage);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting profile completion status for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving profile completion status" });
        }
    }
}
