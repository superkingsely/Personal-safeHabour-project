using SafeHabour.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text;

namespace SafeHabour.Application.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ISendGridClient _sendGridClient;
    private readonly string _templatesPath;

    public EmailService(
        ILogger<EmailService> logger, 
        IConfiguration configuration,
        ISendGridClient sendGridClient)
    {
        _logger = logger;
        _configuration = configuration;
        _sendGridClient = sendGridClient;
        _templatesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates");
    }

    /// <summary>
    /// Sends a two-factor authentication code to the user's email
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="code">Six-digit verification code</param>
    /// <param name="userName">User's name for personalization</param>
    /// <returns>Success status</returns>
    public async Task<bool> SendTwoFactorCodeAsync(string email, string code, string userName)
    {
        try
        {
            _logger.LogInformation("Sending two-factor authentication code to {Email}", email);

            // Load and process the email template
            var htmlContent = await LoadAndProcessTemplate("two-factor-auth.html", new Dictionary<string, string>
            {
                { "UserName", userName },
                { "VerificationCode", code },
                { "Year", DateTime.UtcNow.Year.ToString() }
            });

            // Create SendGrid message
            var from = new EmailAddress(
                _configuration["SendGrid:FromEmail"] ?? "noreply@safeharbour.com",
                _configuration["SendGrid:FromName"] ?? "SafeHarbour Security"
            );
            var to = new EmailAddress(email, userName);
            var subject = "Your SafeHarbour Two-Factor Authentication Code";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

            // Add tracking and settings
            msg.SetClickTracking(false, false);
            msg.SetOpenTracking(false);
            msg.SetGoogleAnalytics(false);
            msg.SetSubscriptionTracking(false);

            // Send the email
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Two-factor authentication code sent successfully to {Email}", email);
                return true;
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send two-factor authentication code to {Email}. Status: {StatusCode}, Response: {Response}", 
                    email, response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending two-factor authentication code to {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Sends approval/rejection notification to user
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="message">Email message</param>
    /// <param name="userName">User's name for personalization</param>
    /// <returns>Success status</returns>
    public async Task<bool> SendApprovalNotificationAsync(string email, string subject, string message, string userName)
    {
        try
        {
            _logger.LogInformation("Sending approval notification to {Email} with subject: {Subject}", email, subject);

            // Determine notification type and styling based on subject/message content
            var notificationType = DetermineNotificationType(subject, message);
            var templateData = GetApprovalTemplateData(notificationType, subject, message, userName);

            // Load and process the email template
            var htmlContent = await LoadAndProcessTemplate("approval-notification.html", templateData);

            // Create SendGrid message
            var from = new EmailAddress(
                _configuration["SendGrid:FromEmail"] ?? "noreply@safeharbour.com",
                _configuration["SendGrid:FromName"] ?? "SafeHarbour Team"
            );
            var to = new EmailAddress(email, userName);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

            // Add tracking and settings
            msg.SetClickTracking(false, false);
            msg.SetOpenTracking(false);
            msg.SetGoogleAnalytics(false);
            msg.SetSubscriptionTracking(false);

            // Send the email
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Approval notification sent successfully to {Email}", email);
                return true;
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send approval notification to {Email}. Status: {StatusCode}, Response: {Response}", 
                    email, response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending approval notification to {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Sends email confirmation link to the user's email
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="confirmationToken">Email confirmation token</param>
    /// <param name="userName">User's name for personalization</param>
    /// <param name="userId">User's ID for the confirmation link</param>
    /// <returns>Success status</returns>
    public async Task<bool> SendEmailConfirmationAsync(string email, string confirmationToken, string userName, string userId)
    {
        try
        {
            _logger.LogInformation("Sending email confirmation to {Email}", email);

            // Create confirmation URL - you should adjust this to match your frontend URL
            var baseUrl = _configuration["AppSettings:FrontendUrl"] ?? "https://localhost:3000";
            var confirmationUrl = $"{baseUrl}/confirm-email?userId={userId}&token={Uri.EscapeDataString(confirmationToken)}";

            // Load and process the email template
            var htmlContent = await LoadAndProcessTemplate("email-confirmation.html", new Dictionary<string, string>
            {
                { "UserName", userName },
                { "ConfirmationUrl", confirmationUrl },
                { "Year", DateTime.UtcNow.Year.ToString() }
            });

            // Create SendGrid message
            var from = new EmailAddress(
                _configuration["SendGrid:FromEmail"] ?? "noreply@safeharbour.com",
                _configuration["SendGrid:FromName"] ?? "SafeHarbour Team"
            );
            var to = new EmailAddress(email, userName);
            var subject = "Confirm Your SafeHarbour Account";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

            // Add tracking and settings
            msg.SetClickTracking(false, false);
            msg.SetOpenTracking(false);
            msg.SetGoogleAnalytics(false);
            msg.SetSubscriptionTracking(false);

            // Send the email
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email confirmation sent successfully to {Email}", email);
                return true;
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email confirmation to {Email}. Status: {StatusCode}, Response: {Response}", 
                    email, response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email confirmation to {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Sends password reset link to the user's email
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="resetToken">Password reset token</param>
    /// <param name="userName">User's name for personalization</param>
    /// <returns>Success status</returns>
    public async Task<bool> SendPasswordResetAsync(string email, string resetToken, string userName)
    {
        try
        {
            _logger.LogInformation("Sending password reset email to {Email}", email);

            // Create password reset URL - you should adjust this to match your frontend URL
            var baseUrl = _configuration["AppSettings:FrontendUrl"] ?? "https://localhost:3000";
            var resetUrl = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";

            // Load and process the email template
            var htmlContent = await LoadAndProcessTemplate("password-reset.html", new Dictionary<string, string>
            {
                { "UserName", userName },
                { "ResetUrl", resetUrl },
                { "Year", DateTime.UtcNow.Year.ToString() }
            });

            // Create SendGrid message
            var from = new EmailAddress(
                _configuration["SendGrid:FromEmail"] ?? "noreply@safeharbour.com",
                _configuration["SendGrid:FromName"] ?? "SafeHarbour Security"
            );
            var to = new EmailAddress(email, userName);
            var subject = "Reset Your SafeHarbour Password";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

            // Add tracking and settings
            msg.SetClickTracking(false, false);
            msg.SetOpenTracking(false);
            msg.SetGoogleAnalytics(false);
            msg.SetSubscriptionTracking(false);

            // Send the email
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Password reset email sent successfully to {Email}", email);
                return true;
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send password reset email to {Email}. Status: {StatusCode}, Response: {Response}", 
                    email, response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending password reset email to {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Sends password reset confirmation email to the user
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="userName">User's name for personalization</param>
    /// <returns>Success status</returns>
    public async Task<bool> SendPasswordResetConfirmationAsync(string email, string userName)
    {
        try
        {
            _logger.LogInformation("Sending password reset confirmation email to {Email}", email);

            // Load and process the email template
            var htmlContent = await LoadAndProcessTemplate("password-reset-confirmation.html", new Dictionary<string, string>
            {
                { "UserName", userName },
                { "ResetDateTime", DateTime.UtcNow.ToString("MMMM dd, yyyy 'at' h:mm tt UTC") },
                { "Year", DateTime.UtcNow.Year.ToString() }
            });

            // Create SendGrid message
            var from = new EmailAddress(
                _configuration["SendGrid:FromEmail"] ?? "noreply@safeharbour.com",
                _configuration["SendGrid:FromName"] ?? "SafeHarbour Security"
            );
            var to = new EmailAddress(email, userName);
            var subject = "Your SafeHarbour Password Has Been Reset";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

            // Add tracking and settings
            msg.SetClickTracking(false, false);
            msg.SetOpenTracking(false);
            msg.SetGoogleAnalytics(false);
            msg.SetSubscriptionTracking(false);

            // Send the email
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Password reset confirmation email sent successfully to {Email}", email);
                return true;
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send password reset confirmation email to {Email}. Status: {StatusCode}, Response: {Response}", 
                    email, response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending password reset confirmation email to {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Loads an email template and replaces placeholders with actual values
    /// </summary>
    /// <param name="templateName">Name of the template file</param>
    /// <param name="placeholders">Dictionary of placeholder values</param>
    /// <returns>Processed HTML content</returns>
    private async Task<string> LoadAndProcessTemplate(string templateName, Dictionary<string, string> placeholders)
    {
        try
        {
            var templatePath = Path.Combine(_templatesPath, templateName);
            
            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Email template not found: {TemplatePath}", templatePath);
                return GetFallbackTemplate(placeholders);
            }

            var template = await File.ReadAllTextAsync(templatePath);
            
            // Replace placeholders
            foreach (var placeholder in placeholders)
            {
                template = template.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
            }

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading email template: {TemplateName}", templateName);
            return GetFallbackTemplate(placeholders);
        }
    }

    /// <summary>
    /// Determines the notification type based on subject and message content
    /// </summary>
    /// <param name="subject">Email subject</param>
    /// <param name="message">Email message</param>
    /// <returns>Notification type</returns>
    private static string DetermineNotificationType(string subject, string message)
    {
        var lowerSubject = subject.ToLowerInvariant();
        var lowerMessage = message.ToLowerInvariant();

        if (lowerSubject.Contains("approved") || lowerMessage.Contains("approved") || 
            lowerSubject.Contains("welcome") || lowerMessage.Contains("welcome"))
        {
            return "approved";
        }
        else if (lowerSubject.Contains("rejected") || lowerMessage.Contains("rejected") ||
                 lowerSubject.Contains("denied") || lowerMessage.Contains("denied"))
        {
            return "rejected";
        }
        else
        {
            return "pending";
        }
    }

    /// <summary>
    /// Gets template data for approval notifications based on type
    /// </summary>
    /// <param name="notificationType">Type of notification (approved, rejected, pending)</param>
    /// <param name="subject">Email subject</param>
    /// <param name="message">Email message</param>
    /// <param name="userName">User's name</param>
    /// <returns>Template data dictionary</returns>
    private static Dictionary<string, string> GetApprovalTemplateData(string notificationType, string subject, string message, string userName)
    {
        var templateData = new Dictionary<string, string>
        {
            { "UserName", userName },
            { "Message", message },
            { "Year", DateTime.UtcNow.Year.ToString() }
        };

        switch (notificationType)
        {
            case "approved":
                templateData.Add("StatusClass", "status-approved");
                templateData.Add("StatusIcon", "✅");
                templateData.Add("StatusTitle", "Account Approved!");
                templateData.Add("StatusSubtitle", "Welcome to SafeHarbour");
                break;
            case "rejected":
                templateData.Add("StatusClass", "status-rejected");
                templateData.Add("StatusIcon", "❌");
                templateData.Add("StatusTitle", "Application Update");
                templateData.Add("StatusSubtitle", "Please review the information below");
                break;
            default:
                templateData.Add("StatusClass", "status-pending");
                templateData.Add("StatusIcon", "⏳");
                templateData.Add("StatusTitle", "Status Update");
                templateData.Add("StatusSubtitle", "We'll keep you informed");
                break;
        }

        return templateData;
    }

    /// <summary>
    /// Provides a fallback HTML template when the main template cannot be loaded
    /// </summary>
    /// <param name="placeholders">Placeholder values</param>
    /// <returns>Simple HTML email content</returns>
    private static string GetFallbackTemplate(Dictionary<string, string> placeholders)
    {
        var userName = placeholders.GetValueOrDefault("UserName", "User");
        var message = placeholders.GetValueOrDefault("Message", "");
        var code = placeholders.GetValueOrDefault("VerificationCode", "");
        var confirmationUrl = placeholders.GetValueOrDefault("ConfirmationUrl", "");
        var resetUrl = placeholders.GetValueOrDefault("ResetUrl", "");
        var resetDateTime = placeholders.GetValueOrDefault("ResetDateTime", "");

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><meta charset='UTF-8'></head><body>");
        sb.AppendLine($"<h2>SafeHarbour</h2>");
        sb.AppendLine($"<p>Hello {userName},</p>");
        
        if (!string.IsNullOrEmpty(code))
        {
            sb.AppendLine($"<p>Your verification code is: <strong>{code}</strong></p>");
            sb.AppendLine("<p>This code will expire in 10 minutes.</p>");
        }
        
        if (!string.IsNullOrEmpty(confirmationUrl))
        {
            sb.AppendLine("<p>Please confirm your email address by clicking the link below:</p>");
            sb.AppendLine($"<p><a href='{confirmationUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Confirm Email</a></p>");
            sb.AppendLine("<p>If you cannot click the link, copy and paste this URL into your browser:</p>");
            sb.AppendLine($"<p>{confirmationUrl}</p>");
        }
        
        if (!string.IsNullOrEmpty(resetUrl))
        {
            sb.AppendLine("<p>We received a request to reset your password. Click the link below to reset your password:</p>");
            sb.AppendLine($"<p><a href='{resetUrl}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>");
            sb.AppendLine("<p>If you cannot click the link, copy and paste this URL into your browser:</p>");
            sb.AppendLine($"<p>{resetUrl}</p>");
            sb.AppendLine("<p>This link will expire in 24 hours. If you did not request a password reset, please ignore this email.</p>");
        }

        if (!string.IsNullOrEmpty(resetDateTime))
        {
            sb.AppendLine("<p>Your password has been successfully reset.</p>");
            sb.AppendLine($"<p><strong>Reset Time:</strong> {resetDateTime}</p>");
            sb.AppendLine("<p>If you did not make this change, please contact our support team immediately.</p>");
            sb.AppendLine("<p>For your security, we recommend:</p>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>Using a strong, unique password</li>");
            sb.AppendLine("<li>Enabling two-factor authentication</li>");
            sb.AppendLine("<li>Keeping your account information up to date</li>");
            sb.AppendLine("</ul>");
        }
        
        if (!string.IsNullOrEmpty(message))
        {
            sb.AppendLine($"<p>{message}</p>");
        }
        
        sb.AppendLine("<p>Best regards,<br>SafeHarbour Team</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }
}
