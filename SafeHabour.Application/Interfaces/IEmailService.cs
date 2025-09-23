namespace SafeHabour.Application.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Sends a two-factor authentication code to the user's email
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="code">Six-digit verification code</param>
    /// <param name="userName">User's name for personalization</param>
    /// <returns>Success status</returns>
    Task<bool> SendTwoFactorCodeAsync(string email, string code, string userName);

    /// <summary>
    /// Sends approval/rejection notification to user
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="message">Email message</param>
    /// <param name="userName">User's name for personalization</param>
    /// <returns>Success status</returns>
    Task<bool> SendApprovalNotificationAsync(string email, string subject, string message, string userName);

    /// <summary>
    /// Sends email confirmation link to the user's email
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="confirmationToken">Email confirmation token</param>
    /// <param name="userName">User's name for personalization</param>
    /// <param name="userId">User's ID for the confirmation link</param>
    /// <returns>Success status</returns>
    Task<bool> SendEmailConfirmationAsync(string email, string confirmationToken, string userName, string userId);

    /// <summary>
    /// Sends password reset link to the user's email
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="resetToken">Password reset token</param>
    /// <param name="userName">User's name for personalization</param>
    /// <returns>Success status</returns>
    Task<bool> SendPasswordResetAsync(string email, string resetToken, string userName);

    /// <summary>
    /// Sends password reset confirmation email to the user
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="userName">User's name for personalization</param>
    /// <returns>Success status</returns>
    Task<bool> SendPasswordResetConfirmationAsync(string email, string userName);
}
