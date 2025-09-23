using SafeHabour.Models.Enums;

namespace SafeHabour.Models.Requests;

/// <summary>
/// Request for getting user notifications with pagination and filtering
/// </summary>
public class GetNotificationsRequest
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page (default: 20, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Filter by notification type
    /// </summary>
    public NotificationType? Type { get; set; }

    /// <summary>
    /// Filter by notification priority
    /// </summary>
    public NotificationPriority? Priority { get; set; }

    /// <summary>
    /// Filter by read status
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    /// Filter by delivery status
    /// </summary>
    public bool? IsDelivered { get; set; }

    /// <summary>
    /// Include expired notifications (default: false)
    /// </summary>
    public bool IncludeExpired { get; set; } = false;

    /// <summary>
    /// Sort order: newest, oldest, priority
    /// </summary>
    public string SortOrder { get; set; } = "newest";

    /// <summary>
    /// Validate and normalize the request
    /// </summary>
    public void Validate()
    {
        if (Page < 1) Page = 1;
        if (PageSize < 1) PageSize = 20;
        if (PageSize > 100) PageSize = 100;
        
        if (string.IsNullOrWhiteSpace(SortOrder))
        {
            SortOrder = "newest";
        }
        else
        {
            SortOrder = SortOrder.ToLowerInvariant();
            if (SortOrder != "newest" && SortOrder != "oldest" && SortOrder != "priority")
            {
                SortOrder = "newest";
            }
        }
    }
}

/// <summary>
/// Request for marking notifications as read
/// </summary>
public class MarkNotificationsAsReadRequest
{
    /// <summary>
    /// List of notification IDs to mark as read
    /// </summary>
    public List<Guid> NotificationIds { get; set; } = new();

    /// <summary>
    /// Mark all notifications as read (if true, NotificationIds is ignored)
    /// </summary>
    public bool MarkAll { get; set; } = false;
}

/// <summary>
/// Request for clearing notifications
/// </summary>
public class ClearNotificationsRequest
{
    /// <summary>
    /// Clear all notifications
    /// </summary>
    public bool ClearAll { get; set; } = false;

    /// <summary>
    /// Clear only read notifications
    /// </summary>
    public bool ClearReadOnly { get; set; } = false;

    /// <summary>
    /// Specific notification IDs to clear
    /// </summary>
    public List<Guid>? NotificationIds { get; set; }

    /// <summary>
    /// Clear notifications older than specified days
    /// </summary>
    public int? OlderThanDays { get; set; }

    /// <summary>
    /// Clear notifications of specific type
    /// </summary>
    public NotificationType? Type { get; set; }

    /// <summary>
    /// Clear notifications of specific priority
    /// </summary>
    public NotificationPriority? Priority { get; set; }
}
