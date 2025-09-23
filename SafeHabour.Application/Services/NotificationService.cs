using SafeHabour.Application.Interfaces;
using SafeHabour.Models.Enums;
using Microsoft.Extensions.Logging;
using SafeHabour.Models.Requests;

namespace SafeHabour.Application.Services;

/// <summary>
/// Comprehensive notification service that handles both email and push notifications
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEmailService emailService,
        IPushNotificationService pushNotificationService,
        ILogger<NotificationService> logger)
    {
        _emailService = emailService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    /// <summary>
    /// Send both email and push notification for approval/rejection notifications
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="userId">User's ID for push notification</param>
    /// <param name="subject">Email subject</param>
    /// <param name="message">Notification message</param>
    /// <param name="userName">User's name for personalization</param>
    /// <param name="isApproval">Whether this is an approval (true) or rejection (false)</param>
    /// <returns>Result indicating success of both operations</returns>
    public async Task<NotificationResult> SendApprovalNotificationAsync(
        string email, 
        string userId, 
        string subject, 
        string message, 
        string userName, 
        bool isApproval = true)
    {
        _logger.LogInformation("Sending approval notification to user {UserId} ({Email}). IsApproval: {IsApproval}", userId, email, isApproval);

        var emailTask = _emailService.SendApprovalNotificationAsync(email, subject, message, userName);
        var pushTask = _pushNotificationService.SendTypedNotificationAsync(
            userId,
            NotificationType.BookingUpdates, // Updated to use new notification type
            subject,
            message,
            new Dictionary<string, object>
            {
                { "isApproval", isApproval },
                { "timestamp", DateTime.UtcNow }
            },
            isApproval ? NotificationPriority.Normal : NotificationPriority.High
        );

        var results = await Task.WhenAll(emailTask, pushTask);
        
        return new NotificationResult
        {
            EmailSent = results[0],
            PushSent = results[1],
            Success = results[0] || results[1],
            Message = GetResultMessage(results[0], results[1], "Approval notification")
        };
    }

    /// <summary>
    /// Send job-related notification (new job posting, application update, etc.)
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="userId">User's ID for push notification</param>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="userName">User's name for personalization</param>
    /// <param name="jobId">Related job ID (optional)</param>
    /// <returns>Result indicating success of both operations</returns>
    public async Task<NotificationResult> SendJobNotificationAsync(
        string email,
        string userId,
        string title,
        string message,
        string userName,
        string? jobId = null)
    {
        _logger.LogInformation("Sending job notification to user {UserId} ({Email}). JobId: {JobId}", userId, email, jobId);

        // For job notifications, we might not always send email, depending on user preferences
        // For now, we'll send both
        var emailTask = _emailService.SendApprovalNotificationAsync(email, title, message, userName);
        
        var pushData = new Dictionary<string, object>
        {
            { "timestamp", DateTime.UtcNow }
        };
        if (!string.IsNullOrEmpty(jobId))
        {
            pushData.Add("jobId", jobId);
        }

        var pushTask = _pushNotificationService.SendTypedNotificationAsync(
            userId,
            NotificationType.BookingUpdates, // Updated to use new notification type
            title,
            message,
            pushData,
            NotificationPriority.Normal
        );

        var results = await Task.WhenAll(emailTask, pushTask);
        
        return new NotificationResult
        {
            EmailSent = results[0],
            PushSent = results[1],
            Success = results[0] || results[1],
            Message = GetResultMessage(results[0], results[1], "Job notification")
        };
    }

    /// <summary>
    /// Send payment-related notification
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="userId">User's ID for push notification</param>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="userName">User's name for personalization</param>
    /// <param name="amount">Payment amount (optional)</param>
    /// <returns>Result indicating success of both operations</returns>
    public async Task<NotificationResult> SendPaymentNotificationAsync(
        string email,
        string userId,
        string title,
        string message,
        string userName,
        decimal? amount = null)
    {
        _logger.LogInformation("Sending payment notification to user {UserId} ({Email}). Amount: {Amount}", userId, email, amount);

        var emailTask = _emailService.SendApprovalNotificationAsync(email, title, message, userName);
        
        var pushData = new Dictionary<string, object>
        {
            { "timestamp", DateTime.UtcNow }
        };
        if (amount.HasValue)
        {
            pushData.Add("amount", amount.Value);
        }

        var pushTask = _pushNotificationService.SendTypedNotificationAsync(
            userId,
            NotificationType.PaymentUpdates, // Updated to use new notification type
            title,
            message,
            pushData,
            NotificationPriority.High
        );

        var results = await Task.WhenAll(emailTask, pushTask);
        
        return new NotificationResult
        {
            EmailSent = results[0],
            PushSent = results[1],
            Success = results[0] || results[1],
            Message = GetResultMessage(results[0], results[1], "Payment notification")
        };
    }

    /// <summary>
    /// Send message notification
    /// </summary>
    /// <param name="userId">User's ID for push notification</param>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="fromUserId">ID of the user sending the message</param>
    /// <returns>Result indicating success of push notification</returns>
    public async Task<NotificationResult> SendMessageNotificationAsync(
        string userId,
        string title,
        string message,
        string fromUserId)
    {
        _logger.LogInformation("Sending message notification to user {UserId} from user {FromUserId}", userId, fromUserId);

        // For messages, we typically only send push notifications for immediate attention
        var pushSuccess = await _pushNotificationService.SendTypedNotificationAsync(
            userId,
            NotificationType.Messages, // Updated to use new notification type
            title,
            message,
            new Dictionary<string, object>
            {
                { "fromUserId", fromUserId },
                { "timestamp", DateTime.UtcNow }
            },
            NotificationPriority.Normal
        );

        return new NotificationResult
        {
            EmailSent = false, // Messages typically don't send emails
            PushSent = pushSuccess,
            Success = pushSuccess,
            Message = pushSuccess ? "Message notification sent successfully" : "Failed to send message notification"
        };
    }

    /// <summary>
    /// Get result message based on email and push notification results
    /// </summary>
    /// <param name="emailSent">Whether email was sent successfully</param>
    /// <param name="pushSent">Whether push notification was sent successfully</param>
    /// <param name="notificationType">Type of notification for context</param>
    /// <returns>Descriptive result message</returns>
    private static string GetResultMessage(bool emailSent, bool pushSent, string notificationType)
    {
        return (emailSent, pushSent) switch
        {
            (true, true) => $"{notificationType} sent via both email and push notification",
            (true, false) => $"{notificationType} sent via email only (push notification failed)",
            (false, true) => $"{notificationType} sent via push notification only (email failed)",
            (false, false) => $"Failed to send {notificationType} via both email and push notification"
        };
    }
}
