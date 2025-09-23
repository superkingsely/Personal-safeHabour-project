# Serilog Implementation for SafeHabour API

This document describes the comprehensive Serilog logging implementation added to the SafeHabour API.

## Overview

The SafeHabour API now includes a robust logging solution using Serilog that supports:
- **Database logging** to SQL Server
- **File logging** with rolling files
- **Console logging** for development
- **Structured logging** with searchable parameters
- **Request/Response logging** with correlation IDs
- **Custom enrichers** for additional context

## Configuration

### 1. Configuration Files

Serilog settings are configured in `appsettings.json` and `appsettings.Development.json`:

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "MinimumLevelOverrides": {
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.EntityFrameworkCore": "Warning",
      "System": "Warning"
    },
    "Database": {
      "Enabled": true,
      "ConnectionStringName": "DefaultConnection",
      "TableName": "Logs",
      "SchemaName": "dbo",
      "AutoCreateSqlTable": true,
      "RestrictedToMinimumLevel": "Information"
    },
    "File": {
      "Enabled": true,
      "Path": "wwwroot/logs/SafeHabour-.txt",
      "RollingInterval": "Day",
      "RestrictedToMinimumLevel": "Information",
      "RetainedFileCountLimit": 30,
      "FileSizeLimitBytes": 1073741824,
      "OutputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
    },
    "Console": {
      "Enabled": true,
      "RestrictedToMinimumLevel": "Information",
      "OutputTemplate": "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
    }
  }
}
```

### 2. Strongly-Typed Configuration

The `SerilogSettings` class provides strongly-typed configuration:

- `SerilogSettings` - Main configuration
- `DatabaseSinkSettings` - Database logging options
- `FileSinkSettings` - File logging options  
- `ConsoleSinkSettings` - Console logging options

## Features

### 1. Database Logging

Logs are automatically saved to SQL Server in the `Logs` table with these columns:

| Column | Type | Description |
|--------|------|-------------|
| Id | int IDENTITY | Primary key |
| Message | nvarchar(max) | Rendered log message |
| Level | nvarchar(128) | Log level (Information, Warning, Error, etc.) |
| TimeStamp | datetime2(7) | UTC timestamp |
| Exception | nvarchar(max) | Exception details (if any) |
| UserName | nvarchar(100) | Authenticated user name |
| MachineName | nvarchar(50) | Server machine name |
| RequestId | nvarchar(100) | HTTP request correlation ID |
| RequestPath | nvarchar(500) | HTTP request path |
| SourceContext | nvarchar(200) | Logger category name |
| ActionName | nvarchar(100) | Controller action name |
| ApplicationName | nvarchar(50) | Application identifier |

### 2. File Logging

- **Location**: `wwwroot/logs/` folder
- **Naming**: `SafeHabour-{Date}.txt` (e.g., `SafeHabour-20250923.txt`)
- **Rolling**: Daily rotation
- **Retention**: 30 days (configurable)
- **Size Limit**: 1GB per file (configurable)

### 3. Request Logging

All HTTP requests are automatically logged with:
- Request method and path
- Response status code
- Response time in milliseconds
- User agent and host information
- Correlation ID for request tracking

### 4. Enrichers

Automatic enrichment of log entries with:
- **Machine Name**: Server hosting the application
- **Environment Name**: Development, Staging, Production
- **Process ID**: Current process identifier
- **Thread ID**: Current thread identifier
- **Client IP**: IP address of the requesting client
- **Request Context**: HTTP request information
- **User Context**: Authenticated user information

## Usage

### 1. Basic Logging

```csharp
public class ExampleController : ControllerBase
{
    private readonly ILogger<ExampleController> _logger;

    public ExampleController(ILogger<ExampleController> logger)
    {
        _logger = logger;
    }

    public IActionResult Get()
    {
        _logger.LogInformation("Getting data for user {UserId}", userId);
        
        try
        {
            // Your logic here
            _logger.LogInformation("Successfully retrieved {Count} records", count);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve data for user {UserId}", userId);
            return StatusCode(500);
        }
    }
}
```

### 2. Structured Logging

Use structured logging for better searchability:

```csharp
_logger.LogInformation("User {UserId} performed action {Action} on resource {ResourceId} at {Timestamp}",
    userId, "Create", resourceId, DateTime.UtcNow);
```

### 3. Scoped Logging

Add context to related log entries:

```csharp
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["UserId"] = userId,
    ["OperationId"] = operationId
}))
{
    _logger.LogInformation("Starting operation");
    // All logs within this scope will include UserId and OperationId
    _logger.LogInformation("Operation completed");
}
```

### 4. Performance Logging

Track operation performance:

```csharp
using var activity = _logger.BeginScope("Database Operation");
var stopwatch = Stopwatch.StartNew();

try
{
    // Your database operation
    stopwatch.Stop();
    
    _logger.LogInformation("Database operation completed in {ElapsedMilliseconds}ms", 
        stopwatch.ElapsedMilliseconds);
}
catch (Exception ex)
{
    stopwatch.Stop();
    _logger.LogError(ex, "Database operation failed after {ElapsedMilliseconds}ms", 
        stopwatch.ElapsedMilliseconds);
    throw;
}
```

## Testing

### Test Endpoints

The `LoggingTestController` provides endpoints to test logging:

- `GET /api/loggingtest/info` - Test information log
- `GET /api/loggingtest/warning` - Test warning log  
- `GET /api/loggingtest/error` - Test error log with exception
- `POST /api/loggingtest/custom-log` - Test custom structured log
- `GET /api/loggingtest/database-logs` - Test multiple log levels

### Example Test Request

```bash
curl -X POST https://localhost:7000/api/loggingtest/custom-log \
  -H "Content-Type: application/json" \
  -d '{
    "message": "User login successful",
    "userId": "12345",
    "action": "Login"
  }'
```

## Configuration Best Practices

### 1. Development Environment

- Use `Debug` minimum level for detailed diagnostics
- Enable all sinks for comprehensive logging
- Shorter file retention (7 days)

### 2. Production Environment

- Use `Information` minimum level for performance
- Longer file retention (30+ days)
- Monitor database log table size
- Consider async logging for high-traffic scenarios

### 3. Log Levels

- **Debug**: Detailed flow information for debugging
- **Information**: General application flow and business events
- **Warning**: Unexpected situations that don't stop the application
- **Error**: Error events that don't stop the application
- **Fatal**: Critical errors that may cause the application to terminate

## Monitoring and Maintenance

### 1. Database Maintenance

- Monitor the `Logs` table size
- Implement log archival strategy for old logs
- Consider partitioning for high-volume applications

### 2. File Maintenance

- Monitor disk space in `wwwroot/logs/`
- Adjust retention policies based on requirements
- Consider external log shipping for analysis

### 3. Performance Monitoring

- Monitor logging overhead in production
- Use async logging for high-throughput scenarios
- Adjust minimum log levels to balance detail vs. performance

## Security Considerations

1. **Sensitive Data**: Never log passwords, tokens, or sensitive personal information
2. **PII**: Be careful with personally identifiable information in logs
3. **Access Control**: Secure access to log files and database
4. **Compliance**: Ensure logging practices meet regulatory requirements

## Troubleshooting

### Common Issues

1. **Database Connection**: Ensure connection string is correct
2. **Permissions**: Verify write permissions to log directory
3. **Table Creation**: Check if Logs table was created successfully
4. **File Locks**: Ensure no processes are locking log files

### Diagnostic Commands

```bash
# Check log files
ls -la wwwroot/logs/

# Check database table
SELECT TOP 10 * FROM Logs ORDER BY TimeStamp DESC;

# Check application logs for Serilog errors
grep -i "serilog" wwwroot/logs/SafeHabour-*.txt
```
