using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeHabour.Application.Interfaces;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;

namespace SafeHabour.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TwoFactorController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public TwoFactorController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Verifies a two-factor authentication code and completes the login process
    /// </summary>
    /// <param name="request">Two-factor verification request</param>
    /// <returns>Service result with token if successful</returns>
    [HttpPost("verify")]
    public async Task<ActionResult<ServiceResult<object>>> VerifyTwoFactorCode([FromBody] VerifyTwoFactorCodeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ServiceResult<object>.FailureResult("Invalid request data",
                ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));
        }

        var result = await _authenticationService.VerifyTwoFactorCodeAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Updates the two-factor authentication setting for the current user
    /// </summary>
    /// <param name="request">Two-factor setting update request</param>
    /// <returns>Updated user information</returns>
    [HttpPut("settings")]
    [Authorize]
    public async Task<ActionResult<ServiceResult<UserDto>>> UpdateTwoFactorSetting([FromBody] UpdateTwoFactorSettingRequest request)
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

        var result = await _authenticationService.UpdateTwoFactorSettingAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
