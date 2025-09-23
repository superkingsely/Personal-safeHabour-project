using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;

namespace SafeHabour.Application.Interfaces;

public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    /// <param name="request">Login request containing email and password</param>
    /// <returns>Service result with token and user information</returns>
    Task<ServiceResult<object>> LoginAsync(LoginRequest request);

    /// <summary>
    /// Creates a new client user account
    /// </summary>
    /// <param name="request">Client user creation request</param>
    /// <returns>Service result with token and user information</returns>
    Task<ServiceResult<object>> CreateClientUserAsync(CreateClientUserRequest request);

    /// <summary>
    /// Creates a new service worker user account
    /// </summary>
    /// <param name="request">Service worker user creation request</param>
    /// <returns>Service result with token and user information</returns>
    Task<ServiceResult<object>> CreateServiceWorkerUserAsync(CreateServiceWorkerUserRequest request);

    /// <summary>
    /// Logs out the current user
    /// </summary>
    /// <param name="userId">User ID to logout</param>
    /// <returns>Success status</returns>
    Task<ServiceResult<object>> LogoutAsync(string userId);

    /// <summary>
    /// Refreshes an expired token
    /// </summary>
    /// <param name="token">Expired token</param>
    /// <returns>New service result with fresh tokens</returns>
    Task<ServiceResult<object>> RefreshTokenAsync(string token);

    /// <summary>
    /// Confirms user email address
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="token">Email confirmation token</param>
    /// <returns>Success status</returns>
    Task<ServiceResult<object>> ConfirmEmailAsync(string userId, string token);

    /// <summary>
    /// Sends password reset email
    /// </summary>
    /// <param name="email">User email address</param>
    /// <returns>Success status</returns>
    Task<ServiceResult<object>> ForgotPasswordAsync(string email);

    /// <summary>
    /// Resets user password with token
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="token">Password reset token</param>
    /// <param name="newPassword">New password</param>
    /// <returns>Success status</returns>
    Task<ServiceResult<object>> ResetPasswordAsync(string email, string token, string newPassword);

    /// <summary>
    /// Gets user information by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User DTO</returns>
    Task<UserDto?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Updates user's last login timestamp
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Success status</returns>
    Task<bool> UpdateLastLoginAsync(string userId);

    /// <summary>
    /// Verifies user information by uploading physical documents
    /// </summary>
    /// <param name="request">Verification request containing user ID, document file, and document type</param>
    /// <returns>Service result with success status and updated profile completion</returns>
    Task<ServiceResult<object>> VerifyUserInformationAsync(VerifyUserInformationRequest request);

    /// <summary>
    /// Updates notification settings for a user
    /// </summary>
    /// <param name="request">The update notification setting request</param>
    /// <returns>Service result</returns>
    Task<ServiceResult<UserDto>> UpdateNotificationSettingAsync(UpdateNotificationSettingRequest request);

    /// <summary>
    /// Gets notification settings for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>List of UserNotificationSettingDto</returns>
    Task<List<UserNotificationSettingDto>> GetUserNotificationSettingsAsync(string userId);

    /// <summary>
    /// Verifies a two-factor authentication code and completes the login process
    /// </summary>
    /// <param name="request">The two-factor verification request</param>
    /// <returns>Service result with token and user information</returns>
    Task<ServiceResult<object>> VerifyTwoFactorCodeAsync(VerifyTwoFactorCodeRequest request);

    /// <summary>
    /// Updates the two-factor authentication setting for a user
    /// </summary>
    /// <param name="request">The two-factor setting update request</param>
    /// <returns>Service result with success status</returns>
    Task<ServiceResult<UserDto>> UpdateTwoFactorSettingAsync(UpdateTwoFactorSettingRequest request);

    /// <summary>
    /// Resends email confirmation to a user
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Service result</returns>
    Task<ServiceResult<object>> ResendEmailConfirmationAsync(string email);
}
