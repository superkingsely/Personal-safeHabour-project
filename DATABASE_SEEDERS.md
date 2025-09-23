# SafeHabour Database Seeders

This document provides comprehensive information about the database seeders created for the SafeHabour API project.

## Overview

The seeders are designed to populate the database with initial data for development and testing purposes. All seeders include comprehensive error handling, logging, and duplicate prevention.

## Seeders Created

### 1. RoleSeeder
**Purpose**: Seeds identity roles required by the application.
**File**: `SafeHabour.Data/Seeders/RoleSeeder.cs`

**Roles Created**:
- SuperAdmin
- Admin  
- ClientUser
- ServiceWorker

**Features**:
- Prevents duplicate role creation
- Comprehensive logging
- Error handling

### 2. ClientUserSeeder
**Purpose**: Seeds two client users with specified email addresses.
**File**: `SafeHabour.Data/Seeders/ClientUserSeeder.cs`

**Users Created**:
1. **emmanueltomi50@gmail.com**
   - Name: Emmanuel Tomi
   - Phone: +2348123456789
   - Location: Lagos, Nigeria (Victoria Island)
   - Role: ClientUser
   - Email confirmed and phone verified

2. **adekdebby67@gmail.com**
   - Name: Deborah Adekoya
   - Phone: +2348987654321
   - Location: Abuja, Nigeria (Maitama District)
   - Role: ClientUser
   - Email confirmed and phone verified

**Features**:
- Complete user profiles with realistic data
- Automatic role assignment
- Geographic coordinates for location-based testing
- Default password: "ClientUser123!"

### 3. ServiceWorkerSeeder
**Purpose**: Seeds two service worker users with services and languages.
**File**: `SafeHabour.Data/Seeders/ServiceWorkerSeeder.cs`

**Users Created**:
1. **oreoluwa.ibikunle1@gmail.com**
   - Name: Oreoluwa Ibikunle
   - Phone: +2348123456789
   - Location: Lagos, Nigeria (Ikeja)
   - Services: House Cleaning, Deep Cleaning, Window Cleaning
   - Languages: English (Native), Yoruba (Native)
   - Hourly Rate: ₦2,500.00
   - Role: ServiceWorker

2. **adekdebby67@gmail.com**
   - Name: Adebayo Deborah
   - Phone: +2348987654321
   - Location: Abuja, Nigeria (Wuse District)
   - Services: Babysitting, Tutoring, Pet Care
   - Languages: English (Native), Hausa (Fluent), French (Intermediate)
   - Hourly Rate: ₦3,000.00
   - Role: ServiceWorker

**Features**:
- JSON serialization of services and languages
- Proper ServiceItem and LanguageItem structure
- Realistic service categories and descriptions
- Language proficiency levels
- Complete address information with coordinates

### 4. SuperAdminSeeder
**Purpose**: Seeds two super admin users with specified email addresses.
**File**: `SafeHabour.Data/Seeders/SuperAdminSeeder.cs`

**Users Created**:
1. **emmanueltomi50@gmail.com**
   - Name: Emmanuel Tomi
   - Phone: +2348111111111
   - Role: SuperAdmin

2. **oreoluwa.ibikunle@gmail.com**
   - Name: Oreoluwa Ibikunle
   - Phone: +2348222222222
   - Role: SuperAdmin

**Features**:
- SuperAdmin profile creation
- Automatic role assignment
- Default password: "SuperAdmin123!"

### 5. DatabaseSeeder (Master Seeder)
**Purpose**: Orchestrates all seeding operations in the correct order.
**File**: `SafeHabour.Data/Seeders/DatabaseSeeder.cs`

**Methods**:
- `SeedAllAsync()`: Seeds all data (roles, clients, service workers, super admins)
- `SeedRolesOnlyAsync()`: Seeds only roles (for production)

## Data Structure Details

### ServiceItem Structure
```csharp
public class ServiceItem
{
    public string Name { get; set; }          // e.g., "House Cleaning"
    public string? Description { get; set; }   // e.g., "Professional residential cleaning"
    public string Category { get; set; }      // e.g., "Cleaning"
    public bool IsActive { get; set; }        // true/false
}
```

### LanguageItem Structure
```csharp
public class LanguageItem
{
    public string Name { get; set; }           // e.g., "English"
    public string Code { get; set; }          // e.g., "en"
    public string ProficiencyLevel { get; set; } // "Native", "Fluent", "Intermediate", "Basic"
    public bool IsNative { get; set; }        // true/false
}
```

## Usage Instructions

### Option 1: Use Master Seeder (Recommended)
```csharp
// In Program.cs or Startup.cs
using SafeHabour.Data.Seeders;

// After building the app
await DatabaseSeeder.SeedAllAsync(app.Services, logger);
```

### Option 2: Use Individual Seeders
```csharp
// Get services
var userManager = app.Services.GetRequiredService<UserManager<User>>();
var roleManager = app.Services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
var context = app.Services.GetRequiredService<ApiDbContext>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Seed in order
await RoleSeeder.SeedRolesAsync(roleManager, logger);
await ClientUserSeeder.SeedClientUsersAsync(userManager, context, logger);
await ServiceWorkerSeeder.SeedServiceWorkersAsync(userManager, context, logger);
await SuperAdminSeeder.SeedSuperAdminsAsync(userManager, context, logger);
```

### Option 3: Production Environment (Roles Only)
```csharp
// For production, only seed roles
await DatabaseSeeder.SeedRolesOnlyAsync(app.Services, logger);
```

## Integration Example

Add this to your `Program.cs` after `var app = builder.Build();`:

```csharp
// Database seeding (development environment only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            await DatabaseSeeder.SeedAllAsync(app.Services, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
        }
    }
}
else
{
    // Production: Only seed roles
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            await DatabaseSeeder.SeedRolesOnlyAsync(app.Services, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding roles");
        }
    }
}
```

## Default Passwords

All seeded users have default passwords for development:
- **Client Users**: "ClientUser123!"
- **Service Workers**: "ServiceWorker123!"
- **Super Admins**: "SuperAdmin123!"

⚠️ **Security Note**: Change these passwords in production environments.

## Features

✅ **Duplicate Prevention**: All seeders check for existing records before creating new ones
✅ **Comprehensive Logging**: Detailed logging for monitoring and debugging
✅ **Error Handling**: Robust error handling with detailed error messages
✅ **Realistic Data**: All seeded data includes realistic, complete information
✅ **JSON Serialization**: Proper handling of complex data structures (Services, Languages)
✅ **Role Assignment**: Automatic assignment of appropriate roles to users
✅ **Geographic Data**: Latitude/longitude coordinates for location-based testing
✅ **Email Confirmation**: Pre-confirmed email addresses for immediate testing

## Dependencies

Required NuGet packages:
- Microsoft.AspNetCore.Identity
- Microsoft.Extensions.Logging
- Microsoft.Extensions.DependencyInjection
- System.Text.Json (for JSON serialization)

## Testing

The seeders provide comprehensive test data for:
- User authentication and authorization
- Role-based access control
- Service worker search functionality
- Location-based filtering
- Service and language filtering
- Client-service worker interactions

## Maintenance

When adding new user types or modifying entity structures:
1. Update the relevant seeder class
2. Ensure proper error handling and logging
3. Add duplicate prevention logic
4. Update this documentation
5. Test in development environment before deployment
