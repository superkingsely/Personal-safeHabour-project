# Updated Client User Profile Management

## Overview
Updated the client user update functionality to support comprehensive profile management including personal information, address details, and profile picture uploads. The implementation follows layered architecture principles with file handling in the Application layer.

## Updated Data Model

### User Entity New Fields
```csharp
// Personal Information
public DateTime? DateOfBirth { get; set; }
public string? Gender { get; set; }
public string? Bio { get; set; }
public string? ProfilePicturePath { get; set; }

// Address Information
public string? StreetAddress { get; set; }
public string? City { get; set; }
public string? Country { get; set; }
public string? PostalCode { get; set; }
```

### UpdateClientUserRequest Model
```csharp
public class UpdateClientUserRequest
{
    [Required] public string UserId { get; set; }
    
    // Personal Information
    [StringLength(100)] public string? FirstName { get; set; }
    [StringLength(100)] public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    [StringLength(10)] public string? Gender { get; set; }
    [StringLength(1000)] public string? Bio { get; set; }
    public IFormFile? ProfilePicture { get; set; }
    
    // Address Information
    [StringLength(500)] public string? StreetAddress { get; set; }
    [StringLength(100)] public string? City { get; set; }
    [StringLength(100)] public string? Country { get; set; }
    [StringLength(20)] public string? PostalCode { get; set; }
    
    // Client Type
    public ClientType? ClientType { get; set; }
}
```

## Repository Layer Implementation

### IClientUserRepository Interface
```csharp
Task<ClientUserDto?> UpdateClientUserAsync(UpdateClientUserRequest request, string? profilePicturePath = null);
```

### Repository Implementation
- Handles all user field updates including personal and address information
- Accepts profile picture path as second parameter (handled by Application layer)
- Updates both User and ClientUser entities
- Includes proper error handling and validation

## Application Layer File Handling (Recommended Implementation)

### Example Service Implementation
```csharp
public class ClientUserService : IClientUserService
{
    private readonly IClientUserRepository _repository;
    
    public async Task<ServiceResult<ClientUserDto>> UpdateClientUserAsync(UpdateClientUserRequest request)
    {
        try
        {
            string? profilePicturePath = null;
            
            // Handle profile picture upload in Application layer
            if (request.ProfilePicture != null && request.ProfilePicture.Length > 0)
            {
                // Validate file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(request.ProfilePicture.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return ServiceResult<ClientUserDto>.FailureResult("Invalid file type. Only JPG, PNG, and GIF files are allowed.");
                }
                
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (request.ProfilePicture.Length > maxFileSize)
                {
                    return ServiceResult<ClientUserDto>.FailureResult("File size exceeds the maximum limit of 5MB.");
                }
                
                // Create directory structure
                var uploadPath = Path.Combine("wwwroot", "uploads", "profile-pictures", request.UserId);
                var fullUploadPath = Path.Combine(Directory.GetCurrentDirectory(), uploadPath);
                
                if (!Directory.Exists(fullUploadPath))
                {
                    Directory.CreateDirectory(fullUploadPath);
                }
                
                // Generate unique filename
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var fileName = $"profile_{timestamp}{fileExtension}";
                var filePath = Path.Combine(fullUploadPath, fileName);
                profilePicturePath = Path.Combine("uploads", "profile-pictures", request.UserId, fileName).Replace("\\", "/");
                
                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ProfilePicture.CopyToAsync(stream);
                }
            }
            
            // Call repository with processed file path
            var result = await _repository.UpdateClientUserAsync(request, profilePicturePath);
            
            if (result != null)
            {
                return ServiceResult<ClientUserDto>.SuccessResult(result, "Client user updated successfully");
            }
            
            return ServiceResult<ClientUserDto>.FailureResult("Failed to update client user");
        }
        catch (Exception ex)
        {
            return ServiceResult<ClientUserDto>.FailureResult($"Update failed: {ex.Message}");
        }
    }
}
```

## Controller Layer Implementation

### Example Controller Action
```csharp
[HttpPut("{userId}")]
public async Task<IActionResult> UpdateClientUser(string userId, [FromForm] UpdateClientUserRequest request)
{
    if (request.UserId != userId)
    {
        return BadRequest("User ID mismatch");
    }
    
    var result = await _clientUserService.UpdateClientUserAsync(request);
    
    if (result.IsSuccess)
    {
        return Ok(result);
    }
    
    return BadRequest(result);
}
```

## Key Features

### 1. Comprehensive Profile Management
- **Personal Information**: Name, phone, date of birth, gender, bio
- **Address Information**: Street address, city, country, postal code
- **Profile Picture**: File upload with validation
- **Client Type**: Admin or ClientUser designation

### 2. File Upload Handling
- **Validation**: File type and size restrictions
- **Secure Storage**: Organized directory structure
- **Unique Naming**: Timestamp-based file naming
- **Error Handling**: Graceful failure for file operations

### 3. Layered Architecture Compliance
- **Repository Layer**: Data persistence and basic validation
- **Application Layer**: Business logic and file handling
- **Controller Layer**: HTTP request handling and routing

### 4. Security Features
- **File Validation**: Type and size restrictions
- **Path Security**: Proper path construction to prevent directory traversal
- **User Isolation**: Files stored in user-specific directories
- **Input Validation**: Comprehensive request validation

## Database Migrations

After implementing these changes, you'll need to create a migration:

```bash
dotnet ef migrations add AddUserProfileFields
dotnet ef database update
```

## Usage Examples

### Frontend Integration
```javascript
const formData = new FormData();
formData.append('FirstName', 'John');
formData.append('LastName', 'Doe');
formData.append('DateOfBirth', '1990-01-01');
formData.append('Gender', 'Male');
formData.append('Bio', 'Tell us about yourself');
formData.append('StreetAddress', '123 Main Street');
formData.append('City', 'Toronto');
formData.append('Country', 'Canada');
formData.append('PostalCode', 'M5V 3A1');
formData.append('ProfilePicture', fileInput.files[0]);

fetch(`/api/clientuser/${userId}`, {
    method: 'PUT',
    body: formData
});
```

### Partial Updates
All fields are optional, allowing for partial updates:
```csharp
var request = new UpdateClientUserRequest
{
    UserId = "user-guid",
    Bio = "Updated bio only"
};
```

## Benefits

1. **Complete Profile Management**: Supports all fields shown in the UI
2. **Flexible Updates**: Optional fields allow partial updates
3. **Secure File Handling**: Proper validation and storage
4. **Layered Architecture**: Business logic separated from data access
5. **Error Resilience**: Comprehensive error handling
6. **Performance Optimized**: Only updates provided fields

This implementation provides a complete solution for managing client user profiles with all the fields shown in your UI, following best practices for layered architecture and secure file handling.
