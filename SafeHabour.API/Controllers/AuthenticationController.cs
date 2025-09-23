using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeHabour.Application.Interfaces;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;
using System.Security.Claims;

namespace SafeHabour.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IAuthenticationService authenticationService,
        ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication result with token and user information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ServiceResult<object>.FailureResult("Invalid request data", 
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
            }

            var result = await _authenticationService.LoginAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            // If 2FA is required, return 202 Accepted
            if (result.RequiresTwoFactor)
            {
                return Accepted(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login for email: {Email}", request.Email);
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred during login"));
        }
    }

    /// <summary>
    /// Create a new client user account
    /// </summary>
    /// <param name="request">Client user registration data</param>
    /// <returns>Authentication result with token and user information</returns>
    [HttpPost("register/client")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterClient([FromBody] CreateClientUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ServiceResult<object>.FailureResult("Invalid request data",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
            }

            var result = await _authenticationService.CreateClientUserAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Created($"/api/users/{result.User?.Id}", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during client registration for email: {Email}", request.Email);
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred during registration"));
        }
    }

    /// <summary>
    /// Create a new service worker user account
    /// </summary>
    /// <param name="request">Service worker registration data</param>
    /// <returns>Authentication result with token and user information</returns>
    [HttpPost("register/service-worker")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterServiceWorker([FromBody] CreateServiceWorkerUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ServiceResult<object>.FailureResult("Invalid request data",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
            }

            var result = await _authenticationService.CreateServiceWorkerUserAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Created($"/api/users/{result.User?.Id}", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during service worker registration for email: {Email}", request.Email);
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred during registration"));
        }
    }

    /// <summary>
    /// Logout the current user
    /// </summary>
    /// <returns>Success status</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ServiceResult<object>.FailureResult("User not found in token"));
            }

            var result = await _authenticationService.LogoutAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during logout");
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred during logout"));
        }
    }

    /// <summary>
    /// Refresh an expired token
    /// </summary>
    /// <param name="request">Token refresh request</param>
    /// <returns>New authentication result with fresh tokens</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(request.Token))
            {
                return BadRequest(ServiceResult<object>.FailureResult("Invalid token provided"));
            }

            var result = await _authenticationService.RefreshTokenAsync(request.Token);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token refresh");
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred during token refresh"));
        }
    }

    /// <summary>
    /// Confirm user email address
    /// </summary>
    /// <param name="request">Email confirmation request</param>
    /// <returns>Success status</returns>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ServiceResult<object>.FailureResult("Invalid request data"));
            }

            var result = await _authenticationService.ConfirmEmailAsync(request.UserId, request.Token);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during email confirmation for user: {UserId}", request.UserId);
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred during email confirmation"));
        }
    }

    /// <summary>
    /// Resend email confirmation
    /// </summary>
    /// <param name="request">Resend email confirmation request</param>
    /// <returns>Success status</returns>
    [HttpPost("resend-email-confirmation")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ServiceResult<object>.FailureResult("Invalid email address"));
            }

            var result = await _authenticationService.ResendEmailConfirmationAsync(request.Email);

            // Always return success to prevent email enumeration
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during resend email confirmation for email: {Email}", request.Email);
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred while processing your request"));
        }
    }

    /// <summary>
    /// Send password reset email
    /// </summary>
    /// <param name="request">Forgot password request</param>
    /// <returns>Success status</returns>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ServiceResult<object>.FailureResult("Invalid email address"));
            }

            var result = await _authenticationService.ForgotPasswordAsync(request.Email);

            // Always return success to prevent email enumeration
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during forgot password for email: {Email}", request.Email);
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred while processing your request"));
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    /// <param name="request">Password reset request</param>
    /// <returns>Success status</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ServiceResult<object>.FailureResult("Invalid request data",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
            }

            var result = await _authenticationService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during password reset for email: {Email}", request.Email);
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred during password reset"));
        }
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>User information</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ServiceResult<UserDto>.FailureResult("User not found in token"));
            }

            var user = await _authenticationService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(ServiceResult<UserDto>.FailureResult("User not found"));
            }

            var result = ServiceResult<UserDto>.SuccessResult(user, "User retrieved successfully");
            result.User = user;
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving current user");
            return StatusCode(500, ServiceResult<UserDto>.FailureResult("An error occurred while retrieving user information"));
        }
    }

    /// <summary>
    /// Verify user information by uploading documents
    /// </summary>
    /// <param name="request">Document verification request</param>
    /// <returns>Verification result</returns>
    [HttpPost("verify-documents")]
    [Authorize]
    public async Task<IActionResult> VerifyUserInformation([FromForm] VerifyUserInformationRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ServiceResult<object>.FailureResult("User not found in token"));
            }

            // Set the user ID from the token
            request.UserId = userId;

            if (!ModelState.IsValid)
            {
                return BadRequest(ServiceResult<object>.FailureResult("Invalid request data",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
            }

            var result = await _authenticationService.VerifyUserInformationAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during document verification");
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred during document verification"));
        }
    }

    /// <summary>
    /// Update notification settings
    /// </summary>
    /// <param name="request">Notification setting update request</param>
    /// <returns>Updated user information</returns>
    [HttpPut("notification-settings")]
    [Authorize]
    public async Task<IActionResult> UpdateNotificationSettings([FromBody] UpdateNotificationSettingRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ServiceResult<UserDto>.FailureResult("User not found in token"));
            }

            // Set the user ID from the token
            request.UserId = userId;

            if (!ModelState.IsValid)
            {
                return BadRequest(ServiceResult<UserDto>.FailureResult("Invalid request data",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
            }

            var result = await _authenticationService.UpdateNotificationSettingAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating notification settings");
            return StatusCode(500, ServiceResult<UserDto>.FailureResult("An error occurred while updating notification settings"));
        }
    }

    /// <summary>
    /// Get user notification settings
    /// </summary>
    /// <returns>List of notification settings</returns>
    [HttpGet("notification-settings")]
    [Authorize]
    public async Task<IActionResult> GetNotificationSettings()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ServiceResult<List<UserNotificationSettingDto>>.FailureResult("User not found in token"));
            }

            var settings = await _authenticationService.GetUserNotificationSettingsAsync(userId);

            return Ok(ServiceResult<List<UserNotificationSettingDto>>.SuccessResult(settings, "Notification settings retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notification settings");
            return StatusCode(500, ServiceResult<List<UserNotificationSettingDto>>.FailureResult("An error occurred while retrieving notification settings"));
        }
    }

    /// <summary>
    /// Verify two-factor authentication code
    /// </summary>
    /// <param name="request">Two-factor verification request</param>
    /// <returns>Authentication result with token</returns>
    [HttpPost("verify-two-factor")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyTwoFactorCode([FromBody] VerifyTwoFactorCodeRequest request)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during two-factor verification for email: {Email}", request.Email);
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred during two-factor verification"));
        }
    }

    /// <summary>
    /// Update two-factor authentication settings
    /// </summary>
    /// <param name="request">Two-factor setting update request</param>
    /// <returns>Updated user information</returns>
    [HttpPut("two-factor-settings")]
    [Authorize]
    public async Task<IActionResult> UpdateTwoFactorSettings([FromBody] UpdateTwoFactorSettingRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ServiceResult<UserDto>.FailureResult("User not found in token"));
            }

            // Set the user ID from the token
            request.UserId = userId;

            if (!ModelState.IsValid)
            {
                return BadRequest(ServiceResult<UserDto>.FailureResult("Invalid request data",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
            }

            var result = await _authenticationService.UpdateTwoFactorSettingAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating two-factor settings");
            return StatusCode(500, ServiceResult<UserDto>.FailureResult("An error occurred while updating two-factor settings"));
        }
    }
}
