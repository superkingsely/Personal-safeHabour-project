using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeHabour.Application.Interfaces;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;

namespace SafeHabour.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public NotificationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Gets notification settings for the current user
    /// </summary>
    /// <returns>List of notification settings</returns>
    [HttpGet("settings")]
    public async Task<ActionResult<ServiceResult<List<UserNotificationSettingDto>>>> GetNotificationSettings()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ServiceResult<List<UserNotificationSettingDto>>
            {
                Success = false,
                Message = "User not found"
            });
        }

        var settings = await _authenticationService.GetUserNotificationSettingsAsync(userId);
        return Ok(new ServiceResult<List<UserNotificationSettingDto>>
        {
            Success = true,
            Data = settings,
            Message = "Notification settings retrieved successfully"
        });
    }

    /// <summary>
    /// Updates a specific notification setting for the current user
    /// </summary>
    /// <param name="request">Update notification setting request</param>
    /// <returns>Updated user information with notification settings</returns>
    [HttpPut("settings")]
    public async Task<ActionResult<ServiceResult<UserDto>>> UpdateNotificationSetting([FromBody] UpdateNotificationSettingRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ServiceResult<UserDto>.FailureResult("Invalid request data",
                ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
        }

        // Override the UserId from the token for security
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResult<UserDto>.FailureResult("User not found"));
        }

        request.UserId = userId;

        var result = await _authenticationService.UpdateNotificationSettingAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
