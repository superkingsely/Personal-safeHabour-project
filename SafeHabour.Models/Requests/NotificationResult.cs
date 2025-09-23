using System;

namespace SafeHabour.Models.Requests;

/// <summary>
/// Result of a notification operation
/// </summary>
public class NotificationResult
{
    /// <summary>
    /// Whether the email notification was sent successfully
    /// </summary>
    public bool EmailSent { get; set; }

    /// <summary>
    /// Whether the push notification was sent successfully
    /// </summary>
    public bool PushSent { get; set; }

    /// <summary>
    /// Overall success status (true if at least one method succeeded)
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Descriptive message about the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional data about the result (optional)
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
}
