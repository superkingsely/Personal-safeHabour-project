# Client User Update Service & Controller Implementation

## Overview
Implemented the complete service layer and API controller endpoints for updating client user profiles, including comprehensive file upload handling and business logic validation.

## Service Layer Implementation

### IClientUserService Interface
```csharp
/// <summary>
/// Updates a client user profile with file upload handling
/// </summary>
/// <param name="request">The update client user request</param>
/// <returns>ServiceResult containing updated ClientUserDto if successful</returns>
Task<ServiceResult<ClientUserDto>> UpdateClientUserAsync(UpdateClientUserRequest request);
```

### ClientUserService Implementation
The service handles:

#### 1. User Validation
- Validates user exists and is active
- Comprehensive logging for audit trail
- Proper error handling and user feedback

#### 2. File Upload Processing
- **File Type Validation**: Only allows `.jpg`, `.jpeg`, `.png`, `.gif`
- **File Size Validation**: Maximum 5MB limit
- **Secure Storage**: Organized directory structure (`wwwroot/uploads/profile-pictures/{userId}/`)
- **Unique Naming**: Timestamp-based file naming (`profile_yyyyMMdd_HHmmss.ext`)
- **Error Recovery**: Proper cleanup on failures

#### 3. Business Logic
- Profile picture processing in Application layer
- Passes processed file path to Repository layer
- Comprehensive error handling with descriptive messages
- Detailed logging for troubleshooting

```csharp
public async Task<ServiceResult<ClientUserDto>> UpdateClientUserAsync(UpdateClientUserRequest request)
{
    // User validation
    var user = await _userManager.FindByIdAsync(request.UserId);
    if (user == null || !user.IsActive) { /* handle error */ }

    // File upload handling
    if (request.ProfilePicture != null && request.ProfilePicture.Length > 0)
    {
        // Validate file type and size
        // Create secure directory structure
        // Save file with unique naming
        // Generate relative path for database
    }

    // Call repository with processed file path
    var result = await _clientUserRepository.UpdateClientUserAsync(request, profilePicturePath);
    
    return ServiceResult<ClientUserDto>.SuccessResult(result, "Profile updated successfully");
}
```

## Controller Layer Implementation

### Two Update Endpoints

#### 1. User Self-Update Endpoint
```csharp
[HttpPut("profile")]
public async Task<IActionResult> UpdateMyProfile([FromForm] UpdateClientUserRequest request)
```

**Features:**
- Users can update their own profiles
- Automatically sets UserId from JWT claims
- Accepts multipart/form-data for file uploads
- Comprehensive error handling

**Usage:**
```http
PUT /api/clientuser/profile
Content-Type: multipart/form-data

firstName: John
lastName: Doe
dateOfBirth: 1990-01-01
gender: Male
bio: Updated bio
streetAddress: 123 Main St
city: Toronto
country: Canada
postalCode: M5V 3A1
profilePicture: [file]
```

#### 2. Admin Update Endpoint
```csharp
[HttpPut("user/{userId}")]
[Authorize(Roles = "SuperAdmin")]
public async Task<IActionResult> UpdateClientUser(string userId, [FromForm] UpdateClientUserRequest request)
```

**Features:**
- SuperAdmin can update any user's profile
- UserId from route parameter for clarity
- Same comprehensive functionality as self-update
- Role-based authorization

**Usage:**
```http
PUT /api/clientuser/user/123e4567-e89b-12d3-a456-426614174000
Content-Type: multipart/form-data
Authorization: Bearer {jwt-token}

firstName: John
lastName: Doe
clientType: Admin
[other fields...]
```

## File Upload Handling

### Directory Structure
```
wwwroot/
└── uploads/
    └── profile-pictures/
        └── {userId}/
            ├── profile_20250923_143022.jpg
            ├── profile_20250923_150145.png
            └── ...
```

### File Validation
- **Allowed Types**: JPG, JPEG, PNG, GIF
- **Maximum Size**: 5MB
- **Security**: File extension validation prevents script uploads
- **Organization**: User-specific directories prevent conflicts

### File Naming Convention
- Format: `profile_{timestamp}{extension}`
- Example: `profile_20250923_143022.jpg`
- Ensures uniqueness and prevents overwrites

## Error Handling

### Service Layer Errors
```csharp
// User validation errors
"User not found"
"User account is inactive"

// File validation errors
"Invalid file type. Only JPG, PNG, and GIF files are allowed."
"File size exceeds the maximum limit of 5MB."

// File processing errors
"Failed to save profile picture. Please try again."

// General errors
"An error occurred while updating the profile"
```

### Controller Layer Errors
```csharp
// Authentication errors
400 Bad Request: "Invalid user ID"

// Authorization errors
401 Unauthorized: Not authenticated
403 Forbidden: Insufficient permissions

// Server errors
500 Internal Server Error: "An error occurred while updating your profile"
```

## Security Features

### 1. Authentication & Authorization
- All endpoints require authentication
- Admin endpoint requires SuperAdmin role
- User can only update own profile (unless admin)

### 2. File Security
- File type validation prevents script uploads
- File size limits prevent DoS attacks
- User-specific directories prevent cross-user access
- Secure path construction prevents directory traversal

### 3. Input Validation
- Model validation with data annotations
- Business logic validation in service layer
- Proper error messages without information disclosure

## API Documentation

### Request Format
```javascript
// Frontend JavaScript example
const formData = new FormData();
formData.append('firstName', 'John');
formData.append('lastName', 'Doe');
formData.append('dateOfBirth', '1990-01-01');
formData.append('gender', 'Male');
formData.append('bio', 'Tell us about yourself');
formData.append('streetAddress', '123 Main Street');
formData.append('city', 'Toronto');
formData.append('country', 'Canada');
formData.append('postalCode', 'M5V 3A1');
formData.append('profilePicture', fileInput.files[0]);

fetch('/api/clientuser/profile', {
    method: 'PUT',
    headers: {
        'Authorization': `Bearer ${token}`
    },
    body: formData
});
```

### Response Format
```json
{
    "success": true,
    "message": "Client user profile updated successfully",
    "data": {
        "id": 1,
        "userId": "123e4567-e89b-12d3-a456-426614174000",
        "clientType": 2,
        "firstName": "John",
        "lastName": "Doe",
        "email": "john.doe@example.com",
        "phoneNumber": "1234567890",
        "dateOfBirth": "1990-01-01T00:00:00Z",
        "gender": "Male",
        "bio": "Tell us about yourself",
        "profilePicturePath": "uploads/profile-pictures/123e4567-e89b-12d3-a456-426614174000/profile_20250923_143022.jpg",
        "streetAddress": "123 Main Street",
        "city": "Toronto",
        "country": "Canada",
        "postalCode": "M5V 3A1"
    },
    "errors": []
}
```

## Performance Considerations

### 1. File Processing
- Asynchronous file operations
- Proper stream disposal
- Error cleanup on failures

### 2. Database Operations
- Single transaction for user and client user updates
- Optimized queries with Include() for related data
- Proper change tracking

### 3. Logging
- Structured logging with correlation IDs
- Performance-sensitive operations logged
- Error context preserved

## Testing Checklist

- [ ] **File Upload**: Test various file types and sizes
- [ ] **Validation**: Test field validation and error messages
- [ ] **Security**: Test unauthorized access attempts
- [ ] **Partial Updates**: Test updating individual fields
- [ ] **Error Handling**: Test database and file system errors
- [ ] **Performance**: Test with large files and concurrent requests

## Benefits

1. **Complete Profile Management**: Supports all profile fields
2. **Secure File Handling**: Comprehensive validation and security
3. **Layered Architecture**: Proper separation of concerns
4. **Flexible API**: Support for both self-update and admin update
5. **Error Resilience**: Comprehensive error handling
6. **Performance Optimized**: Efficient file and database operations
7. **Comprehensive Logging**: Full audit trail for troubleshooting

This implementation provides a complete, secure, and maintainable solution for client user profile management with file upload capabilities.
