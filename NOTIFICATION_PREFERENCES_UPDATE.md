# Notification Preferences System Update

## Overview
Updated the notification system to match the UI design with new notification types and proper default settings during user registration.

## Changes Made

### 1. Updated NotificationType Enum

**Previous Types:**
- Message
- JobApproval  
- Payment
- JobPosting
- Review
- System

**New Types (matching UI):**
- **BookingUpdates** - Get notified about booking confirmations, cancellations, and changes
- **Messages** - Receive emails when you get new messages from workers
- **PaymentUpdates** - Get notified about payment processing and receipts
- **PromotionsAndMarketing** - Receive promotional offers and service updates
- **PlatformUpdates** - Get notified when there is updates on the platform
- **System** - General system notifications (account updates, policy changes, etc.)

### 2. Updated Default Notification Settings

When new users register, the system now creates notification preferences matching the UI defaults:

```csharp
// Default Settings (matches UI screenshot)
BookingUpdates: ✅ ENABLED (both email and in-app)
Messages: ❌ DISABLED (both email and in-app)  
PaymentUpdates: ✅ ENABLED (both email and in-app)
PromotionsAndMarketing: ❌ DISABLED (both email and in-app)
PlatformUpdates: ❌ DISABLED (both email and in-app)
System: ✅ ENABLED (both email and in-app) // Always enabled for security
```

### 3. Updated Service References

**NotificationService.cs:**
- `JobApproval` → `BookingUpdates`
- `JobPosting` → `BookingUpdates` 
- `Payment` → `PaymentUpdates`
- `Message` → `Messages`

**PushNotificationService.cs:**
- Updated icon mappings for new notification types:
  - `Messages` → `/icons/message.png`
  - `BookingUpdates` → `/icons/booking.png`
  - `PaymentUpdates` → `/icons/payment.png`
  - `PromotionsAndMarketing` → `/icons/promotion.png`
  - `PlatformUpdates` → `/icons/platform.png`
  - `System` → `/icons/system.png`

## Database Migration

A migration was created to handle the notification type changes:
```bash
dotnet ef migrations add UpdateNotificationTypes --project SafeHabour.Data --startup-project SafeHabour.API
```

### Important Notes

1. **Existing Users**: The migration will need to handle updating existing user notification settings
2. **Backward Compatibility**: Old notification records may need to be mapped to new types
3. **Default Icons**: New icon files will need to be added to the `wwwroot/icons/` directory

## API Endpoints

The notification preferences can be managed through the existing endpoints:

**Get Notification Settings:**
```http
GET /api/notification/settings
Authorization: Bearer {jwt-token}
```

**Update Notification Setting:**
```http
PUT /api/notification/settings
Content-Type: application/json
Authorization: Bearer {jwt-token}

{
    "notificationType": 1, // BookingUpdates
    "emailNotificationEnabled": true,
    "inAppNotificationEnabled": true
}
```

## Usage Examples

### Frontend Integration

```javascript
// Get user's notification preferences
async function getNotificationPreferences() {
    const response = await fetch('/api/notification/settings', {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    return response.json();
}

// Update a specific notification preference  
async function updateNotificationPreference(notificationType, emailEnabled, inAppEnabled) {
    const response = await fetch('/api/notification/settings', {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
            notificationType: notificationType,
            emailNotificationEnabled: emailEnabled,
            inAppNotificationEnabled: inAppEnabled
        })
    });
    return response.json();
}

// Example: Enable booking updates notifications
updateNotificationPreference(1, true, true); // BookingUpdates = 1
```

### Notification Type Mapping

```csharp
public enum NotificationType
{
    BookingUpdates = 1,         // Booking confirmations, cancellations, changes
    Messages = 2,               // New messages from workers
    PaymentUpdates = 3,         // Payment processing and receipts  
    PromotionsAndMarketing = 4, // Promotional offers and service updates
    PlatformUpdates = 5,        // Platform updates and announcements
    System = 6                  // Security and account notifications
}
```

## Benefits

1. **User Experience**: Matches the UI design exactly
2. **Clarity**: Clear, descriptive notification type names
3. **Defaults**: Sensible defaults that prioritize important notifications
4. **Flexibility**: Users can easily customize their preferences
5. **Security**: System notifications always enabled for important updates

## Next Steps

1. **Run Migration**: Apply the database migration to update existing data
2. **Add Icons**: Add the new notification icon files to the project
3. **Test Integration**: Verify frontend integration with new notification types
4. **Update Documentation**: Update any API documentation with new types

This update provides a more intuitive and user-friendly notification system that aligns with modern UX expectations.
