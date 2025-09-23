# SafeHabour API - Search Implementation Summary

## Overview
Successfully implemented comprehensive ServiceWorker search functionality with location-based filtering, pagination, and advanced search parameters.

## Features Implemented

### 1. Serilog Integration
- **Database Logging**: Automatic creation of `Logs` table with structured logging
- **File Logging**: Rolling daily logs in `wwwroot/logs` folder with 30-day retention
- **Console Logging**: Enhanced console output for development
- **Request Tracking**: Comprehensive logging of all API requests and responses

### 2. Data Structure Enhancement
- **Services Field**: Changed from `string` to `List<ServiceItem>` with JSON serialization
- **Languages Field**: Changed from `string` to `List<LanguageItem>` with JSON serialization
- **HourlyRate Support**: Added decimal HourlyRate to ServiceItem for pricing
- **Backward Compatibility**: Maintains compatibility with existing data

### 3. Advanced Search Functionality
- **Location-Based Search**: Uses Haversine algorithm for distance calculation
- **Proximity Filtering**: Filter by radius (in kilometers) from coordinates
- **Fallback Location**: Uses authenticated user's location when coordinates not provided
- **Text Search**: Search across names, bio, services, and languages
- **Multiple Filters**: Service types, languages, hourly rate ranges
- **Sorting Options**: Distance, hourly rate, rating, creation date
- **Pagination**: Configurable page size with metadata

## Technical Implementation

### Core Classes Created/Modified

#### 1. ServiceItem & LanguageItem (Models)
```csharp
public class ServiceItem
{
    public string Name { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
}

public class LanguageItem
{
    public string Name { get; set; } = string.Empty;
}
```

#### 2. SearchServiceWorkersRequest (Request DTO)
- Pagination: Page, PageSize
- Search: SearchTerm
- Filters: Services, Languages, HourlyRate range, Location radius
- Sorting: SortBy, SortDirection

#### 3. ServiceWorkerSearchResultDto (Response DTO)
- All ServiceWorker properties
- Distance calculation (when location provided)
- Enhanced mapping from entity

#### 4. PaginatedResponse<T> (Generic Response)
- Items list
- Pagination metadata (Page, PageSize, TotalCount, TotalPages)
- Location fallback indicator

### Key Algorithms

#### Distance Calculation (Haversine Formula)
```csharp
private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
{
    const double R = 6371; // Earth's radius in kilometers
    var dLat = ToRadians(lat2 - lat1);
    var dLon = ToRadians(lon2 - lon1);
    
    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
    
    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    return R * c;
}
```

#### Location Fallback Logic
1. Use provided coordinates if available
2. Fall back to authenticated user's coordinates
3. Search without location filtering if neither available

### API Endpoints

#### POST /api/serviceworkers/search
- **Purpose**: Advanced search with pagination and location filtering
- **Authentication**: Anonymous (supports authenticated for location fallback)
- **Request**: SearchServiceWorkersRequest
- **Response**: PaginatedResponse<ServiceWorkerSearchResultDto>

#### GET /api/serviceworkers/{id}
- **Purpose**: Get service worker by ID
- **Authentication**: Anonymous
- **Response**: ServiceWorkerDto

#### GET /api/serviceworkers/by-user/{userId}
- **Purpose**: Get service worker by user ID
- **Authentication**: Anonymous
- **Response**: ServiceWorkerDto

#### PUT /api/serviceworkers/update
- **Purpose**: Update service worker profile
- **Authentication**: Required
- **Request**: UpdateServiceWorkerRequest (form data with file upload support)
- **Response**: ServiceWorkerDto

## Search Examples

### Basic Search
```json
{
  "page": 1,
  "pageSize": 10,
  "searchTerm": "",
  "sortBy": "Distance",
  "sortDirection": "Asc"
}
```

### Location-Based Search
```json
{
  "page": 1,
  "pageSize": 10,
  "searchTerm": "cleaning",
  "services": ["House Cleaning"],
  "languages": ["English"],
  "latitude": 51.5074,
  "longitude": -0.1278,
  "radiusKm": 25,
  "minHourlyRate": 15,
  "maxHourlyRate": 50,
  "sortBy": "Distance",
  "sortDirection": "Asc"
}
```

### Rate-Based Search
```json
{
  "page": 1,
  "pageSize": 20,
  "minHourlyRate": 20,
  "maxHourlyRate": 40,
  "sortBy": "HourlyRate",
  "sortDirection": "Asc"
}
```

## Database Changes

### ServiceWorkerUser Entity
- Added `ServicesJson` column for JSON storage
- Added `LanguagesJson` column for JSON storage
- Maintained `Services` and `Languages` properties with automatic serialization
- Added `[NotMapped]` attributes for computed properties

### Logging Infrastructure
- Auto-created `Logs` table with columns:
  - Id, Message, MessageTemplate, Level, TimeStamp, Exception, Properties

## Configuration

### Serilog Setup (Program.cs)
- SQL Server sink for database logging
- File sink for wwwroot/logs with rolling retention
- Console sink for development
- Structured logging with request correlation

### JSON Serialization
- System.Text.Json for Services/Languages
- Automatic serialization in Entity Framework
- Null handling and empty array defaults

## Testing

### HTTP Test File
- Created `ServiceWorkers.http` with test cases:
  - Basic search
  - Location-based search
  - Rate filtering
  - Name search
  - Individual worker retrieval

## Future Enhancements (TODOs)
1. **Rating System**: Complete implementation of user ratings and reviews
2. **File Upload**: Implement profile picture upload functionality
3. **Caching**: Add Redis caching for frequently searched areas
4. **Search Analytics**: Track search patterns and popular services
5. **Real-time Availability**: Integrate worker availability status
6. **Advanced Filters**: Add more filtering options (experience level, verification status)

## Performance Considerations
- Efficient database queries with proper indexing needed
- Location calculations performed in-memory after filtering
- Pagination prevents large result sets
- JSON deserialization only when needed

## Build Status
✅ All projects compile successfully
✅ No errors or warnings
✅ Ready for testing and deployment
