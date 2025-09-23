# SafeHarbour SignalR Push Notifications - Implementation Guide

## Overview
The SafeHarbour API now includes a comprehensive real-time push notification system using SignalR. This implementation provides both standalone push notifications and integrated email + push notification workflows.

## Architecture Components

### 1. SignalR Hub (`NotificationHub.cs`)
- **Location**: `SafeHabour.Application/Hubs/NotificationHub.cs`
- **Purpose**: Manages real-time WebSocket connections
- **Features**:
  - Authenticated connections only
  - Automatic user grouping (personal and role-based)
  - Connection/disconnection logging
  - Group management (join/leave specific notification groups)
  - Notification read status tracking

### 2. Push Notification Service (`PushNotificationService.cs`)
- **Location**: `SafeHabour.Application/Services/PushNotificationService.cs`
- **Purpose**: Core service for sending push notifications
- **Methods**:
  - `SendNotificationToUserAsync()` - Target specific user
  - `SendNotificationToUsersAsync()` - Target multiple users
  - `SendNotificationToRoleAsync()` - Target users by role
  - `SendNotificationToGroupAsync()` - Target custom groups
  - `SendBroadcastNotificationAsync()` - Send to all connected users
  - `SendTypedNotificationAsync()` - Convenience method with notification types

### 3. Comprehensive Notification Service (`NotificationService.cs`)
- **Location**: `SafeHabour.Application/Services/NotificationService.cs`
- **Purpose**: Combines email and push notifications
- **Methods**:
  - `SendTwoFactorNotificationAsync()` - 2FA codes via email + push
  - `SendApprovalNotificationAsync()` - Account approvals/rejections
  - `SendJobNotificationAsync()` - Job-related notifications
  - `SendPaymentNotificationAsync()` - Payment confirmations
  - `SendMessageNotificationAsync()` - Chat/messaging notifications
  - `SendSystemNotificationAsync()` - System-wide announcements

### 4. Push Notification Controller (`PushNotificationController.cs`)
- **Location**: `SafeHabour.API/Controllers/PushNotificationController.cs`
- **Purpose**: API endpoints for testing and managing notifications
- **Endpoints**:
  - `POST /api/pushnotification/test` - Send test notification
  - `POST /api/pushnotification/send-custom` - Send custom notification (Admin only)
  - `POST /api/pushnotification/send-comprehensive` - Test email + push workflow
  - `GET /api/pushnotification/connection-info` - Get connection debugging info

## Configuration

### 1. Program.cs Setup
```csharp
// SignalR Service Registration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Service Registration
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR
    });
});

// Hub Mapping
app.MapHub<NotificationHub>("/notificationHub");
```

### 2. Frontend URLs Configuration
Update the CORS policy in `Program.cs` to include your frontend application URLs:
```csharp
policy.WithOrigins(
    "http://localhost:3000",    // React development
    "https://localhost:3000",   // React development (HTTPS)
    "https://yourdomain.com"    // Production domain
);
```

## Client-Side Integration

### 1. JavaScript/TypeScript Connection
```javascript
import * as signalR from "@microsoft/signalr";

// Create connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://your-api-domain.com/notificationHub", {
        accessTokenFactory: () => {
            // Return your JWT token
            return localStorage.getItem("authToken");
        }
    })
    .withAutomaticReconnect()
    .build();

// Start connection
async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Connected");
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        setTimeout(startConnection, 5000);
    }
}

// Listen for notifications
connection.on("ReceiveNotification", (notification) => {
    console.log("Received notification:", notification);
    
    // Handle notification based on type
    switch (notification.type) {
        case 1: // Message
            showMessageNotification(notification);
            break;
        case 2: // JobApproval
            showJobNotification(notification);
            break;
        case 3: // Payment
            showPaymentNotification(notification);
            break;
        case 6: // System
            showSystemNotification(notification);
            break;
        default:
            showGenericNotification(notification);
    }
});

// Join specific groups
async function joinJobGroup(jobId) {
    await connection.invoke("JoinGroup", `job_${jobId}`);
}

// Mark notification as read
async function markAsRead(notificationId) {
    await connection.invoke("MarkNotificationAsRead", notificationId);
}

startConnection();
```

### 2. React Hook Example
```typescript
import { useEffect, useState } from 'react';
import * as signalR from "@microsoft/signalr";

interface Notification {
    id: string;
    title: string;
    message: string;
    type: number;
    priority: number;
    timestamp: string;
    data?: Record<string, any>;
}

export const useSignalR = (token: string | null) => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [notifications, setNotifications] = useState<Notification[]>([]);
    const [isConnected, setIsConnected] = useState(false);

    useEffect(() => {
        if (!token) return;

        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub", {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);

        newConnection.start()
            .then(() => {
                setIsConnected(true);
                console.log('SignalR Connected');

                newConnection.on("ReceiveNotification", (notification: Notification) => {
                    setNotifications(prev => [notification, ...prev]);
                });

                newConnection.on("JoinedGroup", (groupName: string) => {
                    console.log(`Joined group: ${groupName}`);
                });
            })
            .catch(err => console.error('SignalR Connection Error:', err));

        return () => {
            newConnection.stop();
        };
    }, [token]);

    const sendTestNotification = async () => {
        if (connection) {
            // Call your API endpoint to send test notification
            fetch('/api/pushnotification/test', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
        }
    };

    return {
        connection,
        notifications,
        isConnected,
        sendTestNotification
    };
};
```

## Notification Types and Priorities

### Notification Types (Enum)
```csharp
public enum NotificationType
{
    Message = 1,        // Chat messages, direct communications
    JobApproval = 2,    // Job application approvals/rejections
    Payment = 3,        // Payment confirmations, receipts
    JobPosting = 4,     // New job postings, job updates
    Review = 5,         // Reviews and ratings
    System = 6          // System announcements, maintenance
}
```

### Priority Levels
```csharp
public enum NotificationPriority
{
    Low = 1,        // Non-urgent updates
    Normal = 2,     // Standard notifications
    High = 3,       // Important notifications
    Critical = 4    // Urgent/emergency notifications
}
```

## Usage Examples

### 1. Sending a Two-Factor Authentication Notification
```csharp
// In your authentication controller
var result = await _notificationService.SendTwoFactorNotificationAsync(
    email: user.Email,
    userId: user.Id.ToString(),
    code: "123456",
    userName: user.UserName
);

if (result.Success)
{
    // At least one notification method succeeded
    if (result.EmailSent && result.PushSent)
        _logger.LogInformation("Both email and push notification sent");
    else if (result.EmailSent)
        _logger.LogInformation("Email sent, push failed");
    else
        _logger.LogInformation("Push sent, email failed");
}
```

### 2. Sending Job Application Approval
```csharp
var result = await _notificationService.SendApprovalNotificationAsync(
    email: applicant.Email,
    userId: applicant.Id.ToString(),
    subject: "Job Application Approved!",
    message: "Congratulations! Your application for the position has been approved.",
    userName: applicant.UserName,
    isApproval: true
);
```

### 3. Sending Real-Time Message Notification
```csharp
await _notificationService.SendMessageNotificationAsync(
    userId: recipient.Id.ToString(),
    title: $"New message from {sender.UserName}",
    message: messageContent,
    fromUserId: sender.Id.ToString()
);
```

### 4. System-Wide Announcement
```csharp
var userIds = await _userManager.Users
    .Where(u => u.IsActive)
    .Select(u => u.Id.ToString())
    .ToListAsync();

await _notificationService.SendSystemNotificationAsync(
    userIds: userIds,
    title: "System Maintenance Notice",
    message: "The system will be under maintenance from 2:00 AM to 4:00 AM UTC.",
    priority: NotificationPriority.High
);
```

## Testing

### 1. Test Push Notifications
```bash
# Send test notification to yourself
curl -X POST "https://your-api.com/api/pushnotification/test" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -H "Content-Type: application/json"
```

### 2. Test Custom Notification (Admin)
```bash
curl -X POST "https://your-api.com/api/pushnotification/send-custom" \
     -H "Authorization: Bearer ADMIN_JWT_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "title": "Test Notification",
       "message": "This is a test message",
       "type": 6,
       "priority": 2,
       "broadcastToAll": true
     }'
```

### 3. Test Comprehensive Notification
```bash
curl -X POST "https://your-api.com/api/pushnotification/send-comprehensive" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "type": "approval",
       "subject": "Test Approval",
       "message": "This is a test approval notification",
       "isApproval": true
     }'
```

## Monitoring and Debugging

### 1. Connection Information
```bash
curl -X GET "https://your-api.com/api/pushnotification/connection-info" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 2. Server-Side Logging
All SignalR operations are logged with structured logging:
```csharp
_logger.LogInformation("User {UserId} connected to NotificationHub with connection {ConnectionId}", userId, connectionId);
_logger.LogInformation("Sending notification to user {UserId}: {Title}", userId, notification.Title);
```

### 3. Client-Side Debugging
```javascript
// Enable SignalR logging
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .configureLogging(signalR.LogLevel.Debug) // Add this line
    .build();

// Monitor connection state
connection.onreconnecting((error) => {
    console.log("SignalR reconnecting:", error);
});

connection.onreconnected((connectionId) => {
    console.log("SignalR reconnected:", connectionId);
});

connection.onclose((error) => {
    console.log("SignalR connection closed:", error);
});
```

## Production Considerations

### 1. Scaling
- Use Redis backplane for multiple server instances
- Consider connection limits and memory usage
- Implement proper error handling and retry logic

### 2. Security
- All connections require JWT authentication
- Validate user permissions before sending notifications
- Sanitize notification content to prevent XSS

### 3. Performance
- Use groups efficiently to minimize message broadcast overhead
- Implement notification batching for high-volume scenarios
- Monitor connection count and memory usage

### 4. Reliability
- Implement fallback mechanisms (email if push fails)
- Store notifications in database for offline users
- Handle connection failures gracefully

## Next Steps

1. **Database Persistence**: Add notification storage for offline users
2. **Push Notification Integration**: Integrate with mobile push services (FCM, APNS)
3. **User Preferences**: Allow users to customize notification settings
4. **Analytics**: Track notification delivery rates and user engagement
5. **Templates**: Create notification templates for consistency

---

The SignalR implementation is now ready for production use with comprehensive real-time notification capabilities!
