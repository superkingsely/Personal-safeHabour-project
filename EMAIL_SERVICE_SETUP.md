# SafeHarbour Email Service - Production Setup Guide

## Overview
The SafeHarbour email service has been upgraded to a production-ready implementation with SendGrid integration and professional HTML email templates.

## Features
- ✅ SendGrid integration for reliable email delivery
- ✅ Professional responsive HTML email templates
- ✅ Template system with variable substitution
- ✅ Fallback templates for error scenarios
- ✅ Comprehensive logging and error handling
- ✅ Production-ready configuration management

## Email Templates

### 1. Two-Factor Authentication (`two-factor-auth.html`)
- **Purpose**: Sends 6-digit verification codes for 2FA
- **Features**: 
  - Security-focused messaging
  - Prominent code display
  - 10-minute expiry notice
  - Mobile-responsive design
  - Professional branding

### 2. Approval Notification (`approval-notification.html`)
- **Purpose**: Sends account approval/rejection notifications
- **Features**:
  - Dynamic status indicators (✅ approved, ❌ rejected, ⏳ pending)
  - Conditional styling based on notification type
  - Flexible message content
  - Professional layout with clear next steps

## Configuration Setup

### 1. SendGrid API Key
Get your SendGrid API key from [SendGrid Dashboard](https://app.sendgrid.com/settings/api_keys)

### 2. Update Configuration Files

**appsettings.json:**
```json
{
  "SendGrid": {
    "ApiKey": "YOUR_PRODUCTION_SENDGRID_API_KEY",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "SafeHarbour"
  }
}
```

**appsettings.Development.json:**
```json
{
  "SendGrid": {
    "ApiKey": "YOUR_DEVELOPMENT_SENDGRID_API_KEY",
    "FromEmail": "noreply@yourdomain.com", 
    "FromName": "SafeHarbour Development"
  }
}
```

### 3. Environment Variables (Recommended for Production)
```bash
# Set as environment variable for security
export SendGrid__ApiKey="your-actual-sendgrid-api-key"
export SendGrid__FromEmail="noreply@yourdomain.com"
export SendGrid__FromName="SafeHarbour"
```

## Email Service Usage

### 1. Two-Factor Authentication
```csharp
// Inject IEmailService in your controller/service
private readonly IEmailService _emailService;

// Send 2FA code
var success = await _emailService.SendTwoFactorCodeAsync(
    email: "user@example.com",
    code: "123456",
    userName: "John Doe"
);
```

### 2. Approval Notifications
```csharp
// Send approval notification
var success = await _emailService.SendApprovalNotificationAsync(
    email: "user@example.com",
    subject: "Account Approved - Welcome to SafeHarbour!",
    message: "Your account has been successfully approved. You can now access all features.",
    userName: "John Doe"
);

// Send rejection notification
var success = await _emailService.SendApprovalNotificationAsync(
    email: "user@example.com", 
    subject: "Account Application Update",
    message: "Unfortunately, we cannot approve your account at this time. Please contact support for more information.",
    userName: "John Doe"
);
```

## Template Customization

### Template Variables
Both templates support these variables:
- `{{UserName}}` - User's display name
- `{{Year}}` - Current year for copyright
- `{{VerificationCode}}` - 6-digit code (2FA template only)
- `{{Message}}` - Custom message content (approval template only)

### Approval Template Additional Variables
- `{{StatusClass}}` - CSS class for styling (status-approved, status-rejected, status-pending)
- `{{StatusIcon}}` - Icon for status (✅, ❌, ⏳)
- `{{StatusTitle}}` - Title based on status
- `{{StatusSubtitle}}` - Subtitle based on status

### Customizing Templates
1. Edit HTML files in `wwwroot/templates/`
2. Maintain `{{Variable}}` syntax for placeholders
3. Test with sample data before deploying

## Error Handling

### Fallback System
- If template files are missing, fallback HTML is generated
- All errors are logged with detailed information
- Method returns `false` on failure for proper error handling

### Logging
```csharp
// Logs include:
_logger.LogInformation("Sending two-factor authentication code to {Email}", email);
_logger.LogError("Failed to send email to {Email}. Status: {StatusCode}", email, statusCode);
_logger.LogWarning("Email template not found: {TemplatePath}", templatePath);
```

## Testing

### Development Testing
1. Use SendGrid's test API key
2. Check SendGrid Activity Dashboard for delivery status
3. Monitor application logs for detailed information

### Template Testing
```csharp
// Test with sample data
var testData = new Dictionary<string, string>
{
    { "UserName", "Test User" },
    { "VerificationCode", "123456" },
    { "Year", "2024" }
};
```

## Security Considerations

### 1. API Key Security
- Never commit API keys to source control
- Use environment variables or Azure Key Vault in production
- Rotate API keys regularly

### 2. Email Security
- Disable click tracking for sensitive emails
- Disable open tracking for privacy
- Use HTTPS for all links in templates

### 3. Rate Limiting
SendGrid has rate limits based on your plan:
- Free: 100 emails/day
- Essentials: 40,000+ emails/month
- Monitor usage in SendGrid dashboard

## Deployment Checklist

### Before Going Live:
- [ ] Replace placeholder API key with production key
- [ ] Update FromEmail to your verified domain
- [ ] Test email delivery to various providers (Gmail, Outlook, etc.)
- [ ] Verify SPF/DKIM records are configured in SendGrid
- [ ] Set up domain authentication in SendGrid
- [ ] Test fallback templates work correctly
- [ ] Monitor logs for any errors
- [ ] Verify templates display correctly on mobile devices

## Monitoring & Maintenance

### SendGrid Dashboard
- Monitor delivery rates
- Check bounce and spam rates
- Review activity logs
- Track email performance

### Application Monitoring
- Monitor email service logs
- Set up alerts for failed email sends
- Track success/failure rates
- Monitor template loading errors

## Support

### Common Issues
1. **"SendGrid API key is not configured"** - Check configuration files
2. **"Email template not found"** - Verify template files in wwwroot/templates
3. **"Failed to send email"** - Check SendGrid dashboard and API key validity
4. **Rate limit exceeded** - Upgrade SendGrid plan or implement queuing

### Resources
- [SendGrid Documentation](https://docs.sendgrid.com/)
- [SendGrid API Reference](https://docs.sendgrid.com/api-reference)
- [Template Design Guide](https://docs.sendgrid.com/ui/sending-email/editor)

---

**Note**: Remember to replace all placeholder values with your actual production values before deploying to production.
