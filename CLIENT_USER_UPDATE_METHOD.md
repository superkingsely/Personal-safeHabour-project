# Client User Update Method Implementation

## Overview
Added an `UpdateClientUserAsync` method to the `ClientUserRepository` that allows updating client user information, specifically the client type.

## Implementation Details

### 1. Request Model
Created `UpdateClientUserRequest` class with the following properties:
- `UserId` (required): The user ID of the client user to update
- `ClientType` (required): The new client type to assign

### 2. Repository Interface Update
Added method signature to `IClientUserRepository`:
```csharp
Task<ClientUserDto?> UpdateClientUserAsync(UpdateClientUserRequest request);
```

### 3. Repository Implementation
Added `UpdateClientUserAsync` method to `ClientUserRepository` with:
- Input validation for user ID format
- Database lookup for existing client user
- Client type update with timestamp
- Error handling with null return on failure
- Returns updated `ClientUserDto` on success

## Method Signature

```csharp
public async Task<ClientUserDto?> UpdateClientUserAsync(UpdateClientUserRequest request)
```

## Parameters

- **request**: `UpdateClientUserRequest` containing:
  - `UserId`: String representation of the user's GUID
  - `ClientType`: Enum value (Admin = 1, ClientUser = 2)

## Return Value

- **Success**: Returns `ClientUserDto` with updated information
- **Failure**: Returns `null` if:
  - Invalid user ID format
  - Client user not found
  - Database operation fails

## Usage Example

```csharp
var updateRequest = new UpdateClientUserRequest
{
    UserId = "123e4567-e89b-12d3-a456-426614174000",
    ClientType = ClientType.Admin
};

var updatedClientUser = await _clientUserRepository.UpdateClientUserAsync(updateRequest);

if (updatedClientUser != null)
{
    // Update successful
    Console.WriteLine($"Client user updated: {updatedClientUser.ClientType}");
}
else
{
    // Update failed
    Console.WriteLine("Failed to update client user");
}
```

## Features

1. **Type Safety**: Uses strongly-typed `ClientType` enum
2. **Validation**: Validates user ID format before database operation
3. **Timestamps**: Automatically updates `UpdatedAt` timestamp
4. **Error Handling**: Graceful error handling with null returns
5. **Clean Architecture**: Follows repository pattern with proper separation

## Available Client Types

```csharp
public enum ClientType
{
    Admin = 1,
    ClientUser = 2
}
```

## Database Impact

- Updates the `ClientType` field in the `ClientUsers` table
- Sets `UpdatedAt` timestamp to current UTC time
- Uses Entity Framework's `Update` method for change tracking

## Error Scenarios

1. **Invalid User ID**: If `UserId` cannot be parsed as GUID, returns `null`
2. **Client User Not Found**: If no client user exists for the given user ID, returns `null`
3. **Database Exception**: Any database errors are caught and result in `null` return

## Security Considerations

- Method validates user existence before updating
- Uses parameterized queries through Entity Framework
- No direct SQL injection risks
- Should be combined with authorization checks in service layer

## Integration Notes

This repository method should be integrated with:
- **Service Layer**: Add business logic and authorization
- **Controller Layer**: Add HTTP endpoints for API access
- **Validation**: Add request validation attributes
- **Logging**: Add structured logging for audit trail

## Next Steps

Consider adding:
1. Service layer implementation for business logic
2. Controller endpoint for HTTP access
3. Authorization middleware for security
4. Comprehensive logging for audit trail
5. Unit tests for the update functionality

## Build Status

✅ **Build Successful** - All code compiles without errors
⚠️ **Warnings Present** - Some nullable reference warnings in other entities (not related to this implementation)
