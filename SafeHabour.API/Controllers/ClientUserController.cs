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
public class ClientUserController : ControllerBase
{
    private readonly IClientUserService _clientUserService;
    private readonly ILogger<ClientUserController> _logger;

    public ClientUserController(
        IClientUserService clientUserService,
        ILogger<ClientUserController> logger)
    {
        _clientUserService = clientUserService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user's client profile
    /// </summary>
    /// <returns>Client user details</returns>
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest(new ServiceResult<ClientUserDto>
                {
                    Success = false,
                    Message = "Invalid user ID"
                });
            }

            var result = await _clientUserService.GetClientUserByUserIdAsync(userGuid);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving client profile");
            return StatusCode(500, new ServiceResult<ClientUserDto>
            {
                Success = false,
                Message = "An error occurred while retrieving your profile"
            });
        }
    }

    /// <summary>
    /// Gets client user details by user ID (SuperAdmin only)
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Client user details</returns>
    [HttpGet("user/{userId}")]
    [Authorize(Roles = UserType.SuperAdmin)]
    public async Task<IActionResult> GetClientUserByUserId(Guid userId)
    {
        try
        {
            var result = await _clientUserService.GetClientUserByUserIdAsync(userId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving client user {UserId}", userId);
            return StatusCode(500, new ServiceResult<ClientUserDto>
            {
                Success = false,
                Message = "An error occurred while retrieving client user"
            });
        }
    }

    /// <summary>
    /// Gets client user details by client user ID (SuperAdmin only)
    /// </summary>
    /// <param name="clientUserId">The client user ID</param>
    /// <returns>Client user details</returns>
    [HttpGet("{clientUserId}")]
    [Authorize(Roles = UserType.SuperAdmin)]
    public async Task<IActionResult> GetClientUserById(int clientUserId)
    {
        try
        {
            var result = await _clientUserService.GetClientUserByIdAsync(clientUserId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving client user {ClientUserId}", clientUserId);
            return StatusCode(500, new ServiceResult<ClientUserDto>
            {
                Success = false,
                Message = "An error occurred while retrieving client user"
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
                return BadRequest(new ServiceResult<ClientUserProfileStatus>
                {
                    Success = false,
                    Message = "Invalid user ID"
                });
            }

            var result = await _clientUserService.GetProfileCompletionStatusAsync(userGuid);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving profile completion status");
            return StatusCode(500, new ServiceResult<ClientUserProfileStatus>
            {
                Success = false,
                Message = "An error occurred while retrieving profile completion status"
            });
        }
    }

    /// <summary>
    /// Updates the current user's client profile
    /// </summary>
    /// <param name="request">The update client user request</param>
    /// <returns>Updated client user details</returns>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile([FromForm] UpdateClientUserRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ServiceResult<ClientUserDto>
                {
                    Success = false,
                    Message = "Invalid user ID"
                });
            }

            // Ensure the request UserId matches the authenticated user
            request.UserId = userId;

            var result = await _clientUserService.UpdateClientUserAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating client profile");
            return StatusCode(500, new ServiceResult<ClientUserDto>
            {
                Success = false,
                Message = "An error occurred while updating your profile"
            });
        }
    }
}
