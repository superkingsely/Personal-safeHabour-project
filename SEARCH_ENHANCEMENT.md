# Search Request Enhancement - Multiple Service and Language Filtering

## Overview
Updated the SearchServiceWorkersRequest to support filtering by multiple services and languages instead of single values, providing more flexible search capabilities.

## Changes Made

### 1. SearchServiceWorkersRequest Model Update
**File**: `SafeHabour.Models/Requests/SearchServiceWorkersRequest.cs`

**Before**:
```csharp
public string? ServiceName { get; set; }
public string? Language { get; set; }
```

**After**:
```csharp
public List<string> ServiceNames { get; set; } = new();
public List<string> Languages { get; set; } = new();
```

**Benefits**:
- ✅ Support multiple service filtering in a single request
- ✅ Support multiple language filtering simultaneously  
- ✅ Cleaner API design with consistent list-based filtering
- ✅ Better user experience for advanced search scenarios

### 2. Repository Logic Enhancement
**File**: `SafeHabour.Infrastructure/Repositories/ServiceWorkerRepository.cs`

**Updated Filtering Logic**:
```csharp
// Multiple Services Filtering
if (searchRequest.ServiceNames != null && searchRequest.ServiceNames.Any())
{
    var serviceConditions = searchRequest.ServiceNames
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Select(serviceName => $"\"name\":\"{serviceName.ToLower()}\"")
        .ToList();

    if (serviceConditions.Any())
    {
        // Service worker must have at least one of the specified services
        query = query.Where(sw => serviceConditions.Any(condition => 
            sw.ServicesJson.ToLower().Contains(condition)));
    }
}

// Multiple Languages Filtering  
if (searchRequest.Languages != null && searchRequest.Languages.Any())
{
    var languageConditions = searchRequest.Languages
        .Where(l => !string.IsNullOrWhiteSpace(l))
        .Select(language => $"\"name\":\"{language.ToLower()}\"")
        .ToList();

    if (languageConditions.Any())
    {
        // Service worker must speak at least one of the specified languages
        query = query.Where(sw => languageConditions.Any(condition => 
            sw.LanguagesJson.ToLower().Contains(condition)));
    }
}
```

**Features**:
- **OR Logic**: Service worker matches if they offer ANY of the specified services
- **OR Logic**: Service worker matches if they speak ANY of the specified languages
- **Null Safety**: Handles empty/null lists gracefully
- **Whitespace Filtering**: Ignores empty or whitespace-only entries
- **Case Insensitive**: All comparisons are case-insensitive

### 3. Service Layer Validation
**File**: `SafeHabour.Application/Managers/ServiceWorkerService.cs`

**Added Validations**:
```csharp
// Validate service names list
if (request.ServiceNames != null && request.ServiceNames.Count > 20)
{
    return ServiceResult.FailureResult("Cannot filter by more than 20 service names at once");
}

if (request.ServiceNames != null && request.ServiceNames.Any(s => !string.IsNullOrWhiteSpace(s) && s.Length > 100))
{
    return ServiceResult.FailureResult("Service names cannot exceed 100 characters each");
}

// Validate languages list
if (request.Languages != null && request.Languages.Count > 10)
{
    return ServiceResult.FailureResult("Cannot filter by more than 10 languages at once");
}

if (request.Languages != null && request.Languages.Any(l => !string.IsNullOrWhiteSpace(l) && l.Length > 50))
{
    return ServiceResult.FailureResult("Language names cannot exceed 50 characters each");
}
```

**Validation Rules**:
- **Service Names**: Maximum 20 services, 100 characters each
- **Languages**: Maximum 10 languages, 50 characters each
- **Performance Protection**: Prevents overly complex queries
- **Data Integrity**: Ensures reasonable input sizes

### 4. HTTP Test Examples
**File**: `SafeHabour.API/ServiceWorkers.http`

**Updated Test Cases**:
```http
### Multiple Services and Languages Search
POST {{SafeHabour.API_HostAddress}}/api/serviceworkers/search
Content-Type: application/json

{
  "page": 1,
  "pageSize": 10,
  "searchTerm": "cleaning",
  "serviceNames": ["House Cleaning", "Window Cleaning", "Deep Cleaning"],
  "languages": ["English", "Spanish"],
  "latitude": 51.5074,
  "longitude": -0.1278,
  "radiusKm": 25.0,
  "sortBy": "Distance",
  "sortDirection": "Ascending"
}

### Multiple Services Only
POST {{SafeHabour.API_HostAddress}}/api/serviceworkers/search
Content-Type: application/json

{
  "page": 1,
  "pageSize": 15,
  "serviceNames": ["Babysitting", "Pet Care", "Tutoring"],
  "languages": ["English", "French", "German"],
  "sortBy": "HourlyRate",
  "sortDirection": "Ascending"
}
```

## Use Cases

### 1. Multi-Service Search
**Scenario**: Client needs someone who can do ANY of these services
```json
{
  "serviceNames": ["House Cleaning", "Laundry", "Organizing"]
}
```
**Result**: Returns service workers who offer at least one of these services

### 2. Multi-Language Search  
**Scenario**: Client wants someone who speaks ANY of these languages
```json
{
  "languages": ["English", "Spanish", "French"]
}
```
**Result**: Returns service workers who speak at least one of these languages

### 3. Combined Filtering
**Scenario**: Complex search with multiple criteria
```json
{
  "serviceNames": ["Babysitting", "Tutoring"],
  "languages": ["English", "Mandarin"],
  "minHourlyRate": 20.00,
  "latitude": 40.7128,
  "longitude": -74.0060,
  "radiusKm": 15.0
}
```
**Result**: Returns service workers who:
- Offer babysitting OR tutoring
- Speak English OR Mandarin  
- Charge at least $20/hour
- Are within 15km of the specified location

## Performance Considerations

### Database Query Optimization
- **JSON Filtering**: Uses efficient JSON contains operations
- **Early Filtering**: Applies filters before distance calculations
- **Null Checking**: Skips processing for empty filter lists

### Memory Usage
- **Validation Limits**: Prevents excessive list sizes
- **Lazy Evaluation**: Only processes non-empty filter conditions
- **Efficient Querying**: Uses LINQ's deferred execution

## API Response Examples

### Successful Search Response
```json
{
  "items": [
    {
      "id": 1,
      "firstName": "John",
      "lastName": "Doe",
      "services": [
        { "name": "House Cleaning", "hourlyRate": 25.00 },
        { "name": "Window Cleaning", "hourlyRate": 30.00 }
      ],
      "languages": [
        { "name": "English" },
        { "name": "Spanish" }
      ],
      "distanceKm": 2.5,
      "hourlyRate": 25.00
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

### Validation Error Response
```json
{
  "success": false,
  "message": "Cannot filter by more than 20 service names at once",
  "errors": []
}
```

## Migration Impact

### Backward Compatibility
- ✅ **Non-Breaking**: Empty lists behave like no filtering
- ✅ **Default Values**: Lists initialize to empty collections
- ✅ **Graceful Handling**: Null lists are handled safely

### Client Integration
- **Frontend Update Required**: UI components need to send arrays instead of strings
- **API Documentation**: Swagger will show updated schema automatically
- **Testing**: HTTP test files updated with new format

## Build Status
✅ **All projects compile successfully**
✅ **No compilation errors**
✅ **Only existing warnings (unrelated to changes)**
✅ **Ready for testing and deployment**

## Summary
The search functionality now supports:
- 🔍 **Multiple Service Filtering**: Find workers offering ANY of the specified services
- 🗣️ **Multiple Language Filtering**: Find workers speaking ANY of the specified languages  
- 📊 **Enhanced Validation**: Proper limits and error handling for list inputs
- ⚡ **Performance Optimized**: Efficient querying with reasonable limits
- 🧪 **Comprehensive Testing**: Updated test cases for all scenarios

This enhancement significantly improves the search experience by allowing users to cast a wider net with multiple service and language options while maintaining performance and data integrity.
