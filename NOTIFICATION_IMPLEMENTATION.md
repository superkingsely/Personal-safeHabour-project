# SafeHarbour Push Notification System - Database Persistence Implementation

## Overview
We have successfully enhanced the SafeHarbour notification system with database persistence capabilities, allowing notifications to be stored and retrieved even when users are offline.

## Key Features Implemented

### 1. Database Persistence
- **PushNotification Entity**: Comprehensive entity with all necessary fields for notification tracking
- **Database Integration**: Full Entity Framework Core integration with proper relationships
- **User Association**: Each notification is linked to a specific user via UserId

### 2. Enhanced PushNotificationService
The service now includes database persistence for all notification operations:

#### Core Methods:
- `SendNotificationToUserAsync()` - Enhanced with database saving
- `SendNotificationToUsersAsync()` - Batch notifications with persistence
- `SendNotificationToRoleAsync()` - Role-based notifications with persistence
- `SendBroadcastNotificationAsync()` - Broadcast notifications with persistence

#### New Management Methods:
- `GetUserNotificationsAsync()` - Paginated retrieval with filtering
- `GetUserNotificationStatsAsync()` - User notification statistics
- `MarkNotificationsAsReadAsync()` - Mark specific notifications as read
- `ClearNotificationsAsync()` - Clear notifications based on criteria

### 3. API Endpoints
New controller endpoints for notification management:

#### GET Endpoints:
- `GET /api/pushnotification/my-notifications` - Get paginated notifications
- `GET /api/pushnotification/my-stats` - Get notification statistics
- `GET /api/pushnotification/connection-info` - Get connection status

#### POST Endpoints:
- `POST /api/pushnotification/mark-as-read` - Mark specific notifications as read
- `POST /api/pushnotification/mark-all-as-read` - Mark all notifications as read
- `POST /api/pushnotification/clear` - Clear notifications based on criteria

### 4. Advanced Features

#### Filtering & Pagination:
- Filter by notification type, priority, read status, delivery status
- Sort by newest, oldest, or priority
- Configurable page size (max 100 items)
- Include/exclude expired notifications

#### Statistics:
- Total notification count
- Unread/read counts
- Delivered/undelivered counts
- Counts by type and priority

#### Flexible Clearing:
- Clear all notifications
- Clear only read notifications
- Clear notifications older than X days
- Clear by specific type or priority
- Clear specific notification IDs

## Database Schema

### PushNotification Table
```sql
CREATE TABLE PushNotifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    Type INT NOT NULL,
    Priority INT NOT NULL,
    Data NVARCHAR(MAX) NULL,
    ActionUrl NVARCHAR(500) NULL,
    IconUrl NVARCHAR(500) NULL,
    RequiresAction BIT NOT NULL DEFAULT 0,
    IsRead BIT NOT NULL DEFAULT 0,
    IsDelivered BIT NOT NULL DEFAULT 0,
    ExpiresAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    ReadAt DATETIME2 NULL,
    DeliveredAt DATETIME2 NULL,
    
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    INDEX IX_PushNotifications_UserId (UserId),
    INDEX IX_PushNotifications_CreatedAt (CreatedAt),
    INDEX IX_PushNotifications_IsRead_UserId (IsRead, UserId)
);
```

## Usage Examples

### 1. Send a Notification (with Database Persistence)
```csharp
await _pushNotificationService.SendTypedNotificationAsync(
    userId: "user-id-here",
    type: NotificationType.JobApproval,
    title: "Job Approved!",
    message: "Your job posting has been approved and is now live.",
    data: new Dictionary<string, object> { { "jobId", 123 } },
    priority: NotificationPriority.High
);
```

### 2. Get User Notifications
```csharp
var notifications = await _pushNotificationService.GetUserNotificationsAsync(
    userId: "user-id-here",
    request: new GetNotificationsRequest
    {
        Page = 1,
        PageSize = 20,
        Type = NotificationType.Message,
        IsRead = false,
        SortOrder = "newest"
    }
);
```

### 3. API Usage Examples

#### Get Notifications (Client-side)
```javascript
// Get unread notifications
const response = await fetch('/api/pushnotification/my-notifications?isRead=false&pageSize=10', {
    headers: { 'Authorization': 'Bearer ' + token }
});
const result = await response.json();
console.log(`${result.data.stats.unreadCount} unread notifications`);
```

#### Mark Notifications as Read
```javascript
// Mark specific notifications as read
await fetch('/api/pushnotification/mark-as-read', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
    },
    body: JSON.stringify({
        notificationIds: ['guid1', 'guid2', 'guid3']
    })
});

// Mark all notifications as read
await fetch('/api/pushnotification/mark-all-as-read', {
    method: 'POST',
    headers: { 'Authorization': 'Bearer ' + token }
});
```

#### Clear Old Notifications
```javascript
// Clear notifications older than 30 days
await fetch('/api/pushnotification/clear', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
    },
    body: JSON.stringify({
        olderThanDays: 30
    })
});
```

## Benefits

### 1. Offline User Support
- Notifications are persisted even when users are offline
- Users can retrieve missed notifications when they come back online

### 2. Rich Filtering & Management
- Advanced filtering capabilities for better user experience
- Bulk operations for marking as read or clearing notifications

### 3. Analytics & Insights
- Detailed statistics help understand notification patterns
- Tracking of delivery and read status for system insights

### 4. Performance Optimized
- Database indexes for efficient querying
- Configurable pagination to handle large notification volumes

### 5. Real-time + Persistent
- Best of both worlds: real-time delivery via SignalR + database persistence
- Automatic fallback to database when real-time delivery fails

## Migration Instructions

To apply the database changes:

```bash
# Apply the migration to add PushNotifications table
dotnet ef database update --project SafeHabour.Data --startup-project SafeHabour.API
```

## Next Steps

1. **Run the Migration**: Apply the database migration to create the PushNotifications table
2. **Test the Endpoints**: Use the new API endpoints to verify functionality
3. **Frontend Integration**: Update your frontend to use the new notification management features
4. **Monitor Performance**: Watch the notification delivery and database performance

The system is now ready for production use with comprehensive notification persistence and management capabilities!
