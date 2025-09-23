namespace SafeHabour.Models.Enums;

public enum NotificationType
{
    /// <summary>
    /// Get notified about booking confirmations, cancellations, and changes
    /// </summary>
    BookingUpdates = 1,
    
    /// <summary>
    /// Receive emails when you get new messages from workers
    /// </summary>
    Messages = 2,
    
    /// <summary>
    /// Get notified about payment processing and receipts
    /// </summary>
    PaymentUpdates = 3,
    
    /// <summary>
    /// Receive promotional offers and service updates
    /// </summary>
    PromotionsAndMarketing = 4,
    
    /// <summary>
    /// Get notified when there is updates on the platform
    /// </summary>
    PlatformUpdates = 5,
    
    /// <summary>
    /// General system notifications (account updates, policy changes, etc.)
    /// </summary>
    System = 6
}
