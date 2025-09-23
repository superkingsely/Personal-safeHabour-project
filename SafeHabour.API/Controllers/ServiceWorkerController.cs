using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeHabour.Application.Interfaces;
using SafeHabour.Models.Response;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Enums;
using System.Security.Claims;

namespace SafeHabour.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class ServiceWorkerController : ControllerBase
{
    private readonly IServiceWorkerService _serviceWorkerService;
    private readonly ILogger<ServiceWorkerController> _logger;

    public ServiceWorkerController(
        IServiceWorkerService serviceWorkerService,
        ILogger<ServiceWorkerController> logger)
    {
        _serviceWorkerService = serviceWorkerService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user's service worker profile
    /// </summary>
    /// <returns>Service worker details</returns>
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest(new ServiceResult<ServiceWorkerDto>
                {
                    Success = false,
                    Message = "Invalid user ID"
                });
            }

            var result = await _serviceWorkerService.GetServiceWorkerByUserIdAsync(userGuid);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving service worker profile");
            return StatusCode(500, new ServiceResult<ServiceWorkerDto>
            {
                Success = false,
                Message = "An error occurred while retrieving your profile"
            });
        }
    }

    /// <summary>
    /// Gets service worker details by user ID (SuperAdmin only)
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Service worker details</returns>
    [HttpGet("user/{userId}")]
    [Authorize(Roles = UserType.SuperAdmin)]
    public async Task<IActionResult> GetServiceWorkerByUserId(Guid userId)
    {
        try
        {
            var result = await _serviceWorkerService.GetServiceWorkerByUserIdAsync(userId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving service worker {UserId}", userId);
            return StatusCode(500, new ServiceResult<ServiceWorkerDto>
            {
                Success = false,
                Message = "An error occurred while retrieving service worker"
            });
        }
    }

    /// <summary>
    /// Gets service worker details by service worker ID (SuperAdmin only)
    /// </summary>
    /// <param name="serviceWorkerId">The service worker ID</param>
    /// <returns>Service worker details</returns>
    [HttpGet("{serviceWorkerId}")]
    [Authorize(Roles = UserType.SuperAdmin)]
    public async Task<IActionResult> GetServiceWorkerById(int serviceWorkerId)
    {
        try
        {
            var result = await _serviceWorkerService.GetServiceWorkerByIdAsync(serviceWorkerId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving service worker {ServiceWorkerId}", serviceWorkerId);
            return StatusCode(500, new ServiceResult<ServiceWorkerDto>
            {
                Success = false,
                Message = "An error occurred while retrieving service worker"
            });
        }
    }

    /// <summary>
    /// Gets the current user's profile completion status
    /// </summary>
    /// <returns>Profile completion details</returns>
    [HttpGet("profile/completion")]
    public async Task<IActionResult> GetProfileCompletionStatus()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest(new ServiceResult<ServiceWorkerProfileStatus>
                {
                    Success = false,
                    Message = "Invalid user ID"
                });
            }

            var result = await _serviceWorkerService.GetProfileCompletionStatusAsync(userGuid);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving profile completion status");
            return StatusCode(500, new ServiceResult<ServiceWorkerProfileStatus>
            {
                Success = false,
                Message = "An error occurred while retrieving profile completion status"
            });
        }
    }

    /// <summary>
    /// Updates the current user's service worker profile
    /// </summary>
    /// <param name="request">The update service worker request</param>
    /// <returns>Updated service worker details</returns>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile([FromForm] UpdateServiceWorkerRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ServiceResult<ServiceWorkerDto>
                {
                    Success = false,
                    Message = "Invalid user ID"
                });
            }

            // Ensure the request UserId matches the authenticated user
            request.UserId = userId;

            var result = await _serviceWorkerService.UpdateServiceWorkerAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating service worker profile");
            return StatusCode(500, new ServiceResult<ServiceWorkerDto>
            {
                Success = false,
                Message = "An error occurred while updating your profile"
            });
        }
    }
}
