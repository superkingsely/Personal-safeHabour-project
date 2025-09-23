using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SafeHabour.Application.Hubs;
using SafeHabour.Application.Interfaces;
using SafeHabour.Models.Enums;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;
using System.Security.Claims;

namespace SafeHabour.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PushNotificationController : ControllerBase
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationService _notificationService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<PushNotificationController> _logger;

    public PushNotificationController(
        IPushNotificationService pushNotificationService,
        INotificationService notificationService,
        IHubContext<NotificationHub> hubContext,
        ILogger<PushNotificationController> logger)
    {
        _pushNotificationService = pushNotificationService;
        _notificationService = notificationService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Send a custom notification to a specific user (Admin only)
    /// </summary>
    /// <param name="request">Custom notification request</param>
    /// <returns>Result of the notification</returns>
    [HttpPost("send-custom")]
    [Authorize(Roles = UserType.SuperAdmin)]
    public async Task<ActionResult<ServiceResult<object>>> SendCustomNotification([FromBody] CustomNotificationRequest request)
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

            var notification = new PushNotificationDto
            {
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                Priority = request.Priority,
                Data = request.Data,
                ActionUrl = request.ActionUrl,
                RequiresAction = request.RequiresAction
            };

            bool success;
            
            if (request.BroadcastToAll)
            {
                success = await _pushNotificationService.SendBroadcastNotificationAsync(notification);
            }
            else if (!string.IsNullOrEmpty(request.Role))
            {
                success = await _pushNotificationService.SendNotificationToRoleAsync(request.Role, notification);
            }
            else if (request.UserIds?.Any() == true)
            {
                success = await _pushNotificationService.SendNotificationToUsersAsync(request.UserIds, notification);
            }
            else
            {
                return BadRequest(ServiceResult<object>.FailureResult("Please specify target users, role, or broadcast to all"));
            }

            if (success)
            {
                return Ok(ServiceResult<object>.SuccessResult(new { notificationId = notification.Id }, "Custom notification sent successfully"));
            }
            else
            {
                return StatusCode(500, ServiceResult<object>.FailureResult("Failed to send custom notification"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending custom notification");
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred while sending custom notification"));
        }
    }

    /// <summary>
    /// Get notifications for the current user with pagination and filtering
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="type">Filter by notification type</param>
    /// <param name="priority">Filter by notification priority</param>
    /// <param name="isRead">Filter by read status</param>
    /// <param name="isDelivered">Filter by delivery status</param>
    /// <param name="includeExpired">Include expired notifications (default: false)</param>
    /// <param name="sortOrder">Sort order: newest, oldest, priority (default: newest)</param>
    /// <returns>Paginated notifications</returns>
    [HttpGet("my-notifications")]
    public async Task<ActionResult<ServiceResult<PaginatedNotificationsDto>>> GetMyNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] NotificationType? type = null,
        [FromQuery] NotificationPriority? priority = null,
        [FromQuery] bool? isRead = null,
        [FromQuery] bool? isDelivered = null,
        [FromQuery] bool includeExpired = false,
        [FromQuery] string sortOrder = "newest")
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ServiceResult<PaginatedNotificationsDto>.FailureResult("User not authenticated"));
            }

            var request = new GetNotificationsRequest
            {
                Page = page,
                PageSize = pageSize,
                Type = type,
                Priority = priority,
                IsRead = isRead,
                IsDelivered = isDelivered,
                IncludeExpired = includeExpired,
                SortOrder = sortOrder
            };

            request.Validate();

            var result = await _pushNotificationService.GetUserNotificationsAsync(userId, request);
            return Ok(ServiceResult<PaginatedNotificationsDto>.SuccessResult(result, "Notifications retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting notifications for user");
            return StatusCode(500, ServiceResult<PaginatedNotificationsDto>.FailureResult("An error occurred while retrieving notifications"));
        }
    }

    /// <summary>
    /// Get notification statistics for the current user
    /// </summary>
    /// <returns>Notification statistics</returns>
    [HttpGet("my-stats")]
    public async Task<ActionResult<ServiceResult<NotificationStatsDto>>> GetMyNotificationStats()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ServiceResult<NotificationStatsDto>.FailureResult("User not authenticated"));
            }

            var stats = await _pushNotificationService.GetUserNotificationStatsAsync(userId);
            return Ok(ServiceResult<NotificationStatsDto>.SuccessResult(stats, "Notification statistics retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting notification stats for user");
            return StatusCode(500, ServiceResult<NotificationStatsDto>.FailureResult("An error occurred while retrieving notification statistics"));
        }
    }

    /// <summary>
    /// Mark notifications as read
    /// </summary>
    /// <param name="request">Mark as read request</param>
    /// <returns>Number of notifications marked as read</returns>
    [HttpPost("mark-as-read")]
    public async Task<ActionResult<ServiceResult<object>>> MarkNotificationsAsRead([FromBody] MarkNotificationsAsReadRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ServiceResult<object>.FailureResult("User not authenticated"));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ServiceResult<object>.FailureResult("Invalid request data",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
            }

            var markedCount = await _pushNotificationService.MarkNotificationsAsReadAsync(userId, request.NotificationIds);

            return Ok(ServiceResult<object>.SuccessResult(
                new { markedCount }, 
                $"Successfully marked {markedCount} notification(s) as read"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while marking notifications as read");
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred while marking notifications as read"));
        }
    }

    /// <summary>
    /// Mark all unread notifications as read
    /// </summary>
    /// <returns>Number of notifications marked as read</returns>
    [HttpPost("mark-all-as-read")]
    public async Task<ActionResult<ServiceResult<object>>> MarkAllNotificationsAsRead()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ServiceResult<object>.FailureResult("User not authenticated"));
            }

            // Get all unread notifications for the user and mark them as read
            var allNotifications = await _pushNotificationService.GetUserNotificationsAsync(userId, new GetNotificationsRequest 
            { 
                IsRead = false, 
                PageSize = 1000, // Get a large number to mark all as read
                IncludeExpired = false 
            });
            
            var notificationIds = allNotifications.Notifications.Select(n => n.Id).ToList();
            var markedCount = await _pushNotificationService.MarkNotificationsAsReadAsync(userId, notificationIds);

            return Ok(ServiceResult<object>.SuccessResult(
                new { markedCount }, 
                $"Successfully marked {markedCount} notification(s) as read"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while marking all notifications as read");
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred while marking all notifications as read"));
        }
    }

    /// <summary>
    /// Clear notifications based on criteria
    /// </summary>
    /// <param name="request">Clear notifications request</param>
    /// <returns>Number of notifications cleared</returns>
    [HttpPost("clear")]
    public async Task<ActionResult<ServiceResult<object>>> ClearNotifications([FromBody] ClearNotificationsRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ServiceResult<object>.FailureResult("User not authenticated"));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ServiceResult<object>.FailureResult("Invalid request data",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
            }

            var clearedCount = await _pushNotificationService.ClearNotificationsAsync(userId, request);
            
            return Ok(ServiceResult<object>.SuccessResult(
                new { clearedCount }, 
                $"Successfully cleared {clearedCount} notification(s)"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while clearing notifications");
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred while clearing notifications"));
        }
    }

    /// <summary>
    /// Get connection status information
    /// </summary>
    /// <returns>Connection information</returns>
    [HttpGet("connection-info")]
    public async Task<ActionResult<ServiceResult<object>>> GetConnectionInfo()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ServiceResult<object>.FailureResult("User not authenticated"));
            }

            var isOnline = await _pushNotificationService.IsUserOnlineAsync(userId);
            var onlineUsersCount = await _pushNotificationService.GetOnlineUsersCountAsync();

            var connectionInfo = new
            {
                userId,
                isOnline,
                onlineUsersCount,
                connectionTime = DateTime.UtcNow
            };

            return Ok(ServiceResult<object>.SuccessResult(connectionInfo, "Connection information retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting connection info");
            return StatusCode(500, ServiceResult<object>.FailureResult("An error occurred while retrieving connection information"));
        }
    }
}
