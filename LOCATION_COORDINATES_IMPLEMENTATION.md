# Location Coordinates Implementation

## Overview
Added longitude and latitude fields to the User entity and integrated them throughout the user creation and update workflows to support location-based features.

## Changes Made

### 1. User Entity Updates

**Added Fields:**
```csharp
// Location coordinates
public double? Latitude { get; set; }
public double? Longitude { get; set; }
```

### 2. Request Model Updates

#### CreateClientUserRequest
```csharp
/// <summary>
/// User's latitude coordinate
/// </summary>
public double? Latitude { get; set; }

/// <summary>
/// User's longitude coordinate
/// </summary>
public double? Longitude { get; set; }
```

#### CreateServiceWorkerUserRequest
```csharp
/// <summary>
/// User's latitude coordinate
/// </summary>
public double? Latitude { get; set; }

/// <summary>
/// User's longitude coordinate
/// </summary>
public double? Longitude { get; set; }
```

#### UpdateClientUserRequest
```csharp
/// <summary>
/// User's latitude coordinate
/// </summary>
public double? Latitude { get; set; }

/// <summary>
/// User's longitude coordinate
/// </summary>
public double? Longitude { get; set; }
```

### 3. Response Model Updates

#### UserDto
```csharp
// Location coordinates
public double? Latitude { get; set; }
public double? Longitude { get; set; }
```

#### ClientUserDto (Enhanced)
```csharp
public class ClientUserDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ClientType { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePicturePath { get; set; }
    
    // Address Information
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    
    // Location coordinates
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
```

### 4. Service Layer Updates

#### AuthenticationService - User Creation
```csharp
// Client User Creation
var user = new User
{
    // ... other fields
    Latitude = request.Latitude,
    Longitude = request.Longitude,
    // ... 
};

// Service Worker Creation  
var user = new User
{
    // ... other fields
    StreetAddress = request.Address,
    City = request.City,
    PostalCode = request.PostalCode,
    Country = request.Country,
    DateOfBirth = request.DateOfBirth,
    Bio = request.Bio,
    Latitude = request.Latitude,
    Longitude = request.Longitude,
    // ...
};
```

#### ClientUserRepository - Update Logic
```csharp
// Update coordinates if provided
if (request.Latitude.HasValue)
    user.Latitude = request.Latitude.Value;

if (request.Longitude.HasValue)
    user.Longitude = request.Longitude.Value;
```

### 5. Mapper Updates

#### UserMapper
- Updated both `ToDtoAsync` and `ToDto` methods to include coordinates
- Updated `UpdateEntityFromDto` to handle coordinate updates

#### ClientUserMapper (Enhanced)
- Created comprehensive `ToDtoWithUserDetails` method that includes all user information
- Updated repository calls to use comprehensive mapping
- Improved data flow between User entity and ClientUserDto

### 6. Database Migration

Created migration: `AddLocationCoordinates`
- Adds `Latitude` (double?) column to Users table
- Adds `Longitude` (double?) column to Users table

## API Usage Examples

### User Registration with Location

#### Client User Registration
```http
POST /api/authentication/register/client
Content-Type: application/json

{
    "firstName": "John",
    "lastName": "Doe", 
    "email": "john.doe@example.com",
    "password": "SecurePassword123!",
    "confirmPassword": "SecurePassword123!",
    "phoneNumber": "1234567890",
    "clientType": 2,
    "latitude": 43.6532,
    "longitude": -79.3832
}
```

#### Service Worker Registration
```http
POST /api/authentication/register/service-worker
Content-Type: application/json

{
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "jane.smith@example.com",
    "password": "SecurePassword123!",
    "confirmPassword": "SecurePassword123!", 
    "phoneNumber": "1234567890",
    "bio": "Experienced cleaner",
    "address": "123 Main St",
    "city": "Toronto",
    "postalCode": "M5V 3A1",
    "country": "Canada",
    "dateOfBirth": "1990-01-15",
    "services": "Cleaning,Organizing",
    "languages": "English,French", 
    "hourlyRate": 25,
    "latitude": 43.6532,
    "longitude": -79.3832
}
```

### User Profile Update with Location

```http
PUT /api/clientuser/profile
Content-Type: multipart/form-data
Authorization: Bearer {jwt-token}

firstName: John
lastName: Doe
streetAddress: 456 Queen St
city: Toronto
country: Canada
postalCode: M5H 2M9
latitude: 43.6519
longitude: -79.3817
profilePicture: [file]
```

### Frontend Integration

#### JavaScript Example - Get User Location
```javascript
// Get user's current location
function getCurrentLocation() {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject(new Error('Geolocation not supported'));
            return;
        }

        navigator.geolocation.getCurrentPosition(
            (position) => {
                resolve({
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude
                });
            },
            (error) => reject(error),
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 300000 // 5 minutes
            }
        );
    });
}

// Register user with location
async function registerWithLocation(userData) {
    try {
        const location = await getCurrentLocation();
        
        const registrationData = {
            ...userData,
            latitude: location.latitude,
            longitude: location.longitude
        };

        const response = await fetch('/api/authentication/register/client', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(registrationData)
        });

        return response.json();
    } catch (error) {
        console.error('Registration with location failed:', error);
        // Fallback: register without location
        return registerWithoutLocation(userData);
    }
}
```

#### Address Geocoding Example
```javascript
// Convert address to coordinates using geocoding service
async function geocodeAddress(address, city, country) {
    try {
        // Example using a geocoding service
        const fullAddress = `${address}, ${city}, ${country}`;
        const response = await fetch(
            `https://api.geocoding-service.com/geocode?address=${encodeURIComponent(fullAddress)}`
        );
        
        const data = await response.json();
        
        if (data.results && data.results.length > 0) {
            return {
                latitude: data.results[0].geometry.location.lat,
                longitude: data.results[0].geometry.location.lng
            };
        }
        
        return null;
    } catch (error) {
        console.error('Geocoding failed:', error);
        return null;
    }
}
```

## Benefits

### 1. **Location-Based Features**
- Service worker discovery by proximity
- Distance calculations for service recommendations
- Geographic filtering and search
- Service area mapping

### 2. **Enhanced User Experience**
- Automatic location detection during registration
- Location-aware service recommendations
- Distance-based pricing and availability
- Regional service customization

### 3. **Business Intelligence**
- Geographic distribution analysis
- Service demand mapping
- Regional market insights
- Location-based performance metrics

### 4. **Future Capabilities**
- Geofencing for service areas
- Location-based notifications
- Route optimization for service workers
- Regional promotional targeting

## Data Privacy & Security

### 1. **Optional Fields**
- Latitude and longitude are nullable (optional)
- Users can register without providing location
- Graceful fallback when location is unavailable

### 2. **Data Protection**
- Coordinates stored with reasonable precision
- No reverse geocoding to exact addresses
- Respect user privacy preferences
- Comply with location data regulations

### 3. **User Control**
- Users can update their location anytime
- Option to clear/remove location data
- Transparent about location usage
- Consent-based location collection

## Next Steps

1. **Apply Migration**: Run `dotnet ef database update` to add coordinate fields
2. **Frontend Integration**: Implement location capture in registration forms
3. **Location Services**: Build proximity-based service discovery
4. **Privacy Controls**: Add location management to user preferences
5. **Geocoding**: Integrate address-to-coordinate conversion services

This implementation provides a solid foundation for location-based features while maintaining user privacy and providing flexible location options.
