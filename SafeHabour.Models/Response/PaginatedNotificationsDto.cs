using SafeHabour.Models.Enums;

namespace SafeHabour.Models.Response;

/// <summary>
/// Paginated response for notifications
/// </summary>
public class PaginatedNotificationsDto
{
    /// <summary>
    /// List of notifications for the current page
    /// </summary>
    public List<PushNotificationResponseDto> Notifications { get; set; } = new();

    /// <summary>
    /// Total number of notifications
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Notification statistics for the user
    /// </summary>
    public NotificationStatsDto Stats { get; set; } = new();
}

/// <summary>
/// Notification statistics for a user
/// </summary>
public class NotificationStatsDto
{
    /// <summary>
    /// Total number of notifications
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Number of unread notifications
    /// </summary>
    public int UnreadCount { get; set; }

    /// <summary>
    /// Number of read notifications
    /// </summary>
    public int ReadCount { get; set; }

    /// <summary>
    /// Number of delivered notifications
    /// </summary>
    public int DeliveredCount { get; set; }

    /// <summary>
    /// Number of undelivered notifications
    /// </summary>
    public int UndeliveredCount { get; set; }

    /// <summary>
    /// Count of notifications by type
    /// </summary>
    public Dictionary<string, int> CountByType { get; set; } = new();

    /// <summary>
    /// Count of notifications by priority
    /// </summary>
    public Dictionary<string, int> CountByPriority { get; set; } = new();
}
