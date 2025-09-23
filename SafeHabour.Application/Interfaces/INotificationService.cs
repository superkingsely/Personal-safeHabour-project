using SafeHabour.Models.Enums;
using SafeHabour.Models.Requests;

namespace SafeHabour.Application.Interfaces;

/// <summary>
/// Comprehensive notification service interface that handles both email and push notifications
/// </summary>
public interface INotificationService
{
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
    Task<NotificationResult> SendApprovalNotificationAsync(string email, string userId, string subject, string message, string userName, bool isApproval = true);

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
    Task<NotificationResult> SendJobNotificationAsync(string email, string userId, string title, string message, string userName, string? jobId = null);

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
    Task<NotificationResult> SendPaymentNotificationAsync(string email, string userId, string title, string message, string userName, decimal? amount = null);

    /// <summary>
    /// Send message notification (push only)
    /// </summary>
    /// <param name="userId">User's ID for push notification</param>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="fromUserId">ID of the user sending the message</param>
    /// <returns>Result indicating success of push notification</returns>
    Task<NotificationResult> SendMessageNotificationAsync(string userId, string title, string message, string fromUserId);

}