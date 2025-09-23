# SafeHabour API 🏠

A comprehensive ASP.NET Core Web API for connecting service workers with clients, featuring advanced search capabilities, location-based matching, and secure user management.

## 🌟 Features

### Core Functionality
- **User Management**: Secure authentication and authorization for clients and service workers
- **Service Worker Profiles**: Complete profile management with services, languages, and pricing
- **Advanced Search**: Location-based search with proximity filtering and comprehensive filters
- **Real-time Communication**: SignalR integration for live notifications
- **File Management**: Profile picture uploads with validation and storage
- **Comprehensive Logging**: Structured logging with Serilog (database + file storage)

### Architecture Highlights
- **Clean Architecture**: Separation of concerns with proper layering
- **Repository Pattern**: Data access abstraction
- **Service Layer**: Business logic centralization
- **JWT Authentication**: Secure token-based authentication
- **Entity Framework Core**: Code-first database approach
- **Dependency Injection**: Comprehensive DI container configuration

## 🏗️ Project Structure

```
SafeHabour.API/
├── SafeHabour.API/              # API Layer (Controllers, Middleware)
│   ├── Controllers/             # API Controllers
│   ├── wwwroot/                # Static files and uploads
│   └── Program.cs              # Application entry point
├── SafeHabour.Application/      # Application Layer (Business Logic)
│   ├── Interfaces/             # Service interfaces
│   ├── Managers/               # Service implementations
│   └── Services/               # Supporting services
├── SafeHabour.Infrastructure/   # Infrastructure Layer (Data Access)
│   ├── Interfaces/             # Repository interfaces
│   └── Repositories/           # Repository implementations
├── SafeHabour.Data/            # Data Layer (Entities, Context)
│   └── Entities/               # Database entities
└── SafeHabour.Models/          # Shared Models (DTOs, Requests, Responses)
    ├── Requests/               # Request models
    ├── Response/               # Response models
    └── Configuration/          # Configuration models
```

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/adekniyi/SafeHabourBE.git
   cd SafeHabourBE
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update connection string**
   ```json
   // appsettings.Development.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SafeHabourDB;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Build and run**
   ```bash
   dotnet build
   dotnet run --project SafeHabour.API
   ```

6. **Access the API**
   - API: `https://localhost:7001`
   - Swagger UI: `https://localhost:7001/swagger`

## 📡 API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/refresh` - Refresh JWT token

### Service Workers
- `POST /api/serviceworkers/search` - Advanced search with location filtering
- `GET /api/serviceworkers/{id}` - Get service worker by ID
- `GET /api/serviceworkers/by-user/{userId}` - Get service worker by user ID
- `GET /api/serviceworkers/profile-status/{userId}` - Get profile completion status
- `PUT /api/serviceworkers/update` - Update service worker profile

### Client Users
- `GET /api/clientusers/{id}` - Get client user details
- `PUT /api/clientusers/update` - Update client user profile

### Super Admin
- `GET /api/superadmin/users` - Get all users (paginated)
- `POST /api/superadmin/approve-users` - Approve multiple users
- `GET /api/superadmin/dashboard` - Get admin dashboard data

## 🔍 Advanced Search Features

### Search Parameters
```json
{
  "page": 1,
  "pageSize": 10,
  "searchTerm": "cleaning",
  "serviceCategory": "Cleaning",
  "serviceName": "House Cleaning",
  "languageCode": "en",
  "minHourlyRate": 15.00,
  "maxHourlyRate": 50.00,
  "latitude": 51.5074,
  "longitude": -0.1278,
  "radiusKm": 25.0,
  "sortBy": "Distance",
  "sortDirection": "Ascending",
  "minRating": 4,
  "verifiedOnly": true,
  "availableOnly": true
}
```

### Location-Based Features
- **Haversine Algorithm**: Accurate distance calculations
- **Proximity Filtering**: Search within specified radius
- **Fallback Location**: Uses authenticated user's location when coordinates not provided
- **Geographic Validation**: Validates latitude/longitude ranges

### Sorting Options
- Distance (nearest first)
- Hourly rate (low to high)
- Rating (highest first)
- Name (alphabetical)
- Join date (newest first)

## 🛡️ Security Features

### Authentication & Authorization
- **JWT Tokens**: Secure stateless authentication
- **Role-Based Access**: Different access levels for clients, service workers, and admins
- **Password Hashing**: BCrypt password security
- **Token Refresh**: Automatic token renewal

### Input Validation
- **Request Validation**: Comprehensive parameter validation
- **File Upload Security**: Type and size validation for uploads
- **SQL Injection Protection**: Parameterized queries
- **XSS Protection**: Input sanitization

### Data Protection
- **User Secrets**: Sensitive configuration protection
- **Connection String Security**: Encrypted database connections
- **CORS Configuration**: Controlled cross-origin requests

## 📊 Data Models

### Service Worker Structure
```json
{
  "id": 1,
  "userId": "guid",
  "firstName": "John",
  "lastName": "Doe",
  "bio": "Experienced cleaner...",
  "hourlyRate": 25.50,
  "services": [
    {
      "name": "House Cleaning",
      "hourlyRate": 25.00
    }
  ],
  "languages": [
    {
      "name": "English"
    }
  ],
  "latitude": 51.5074,
  "longitude": -0.1278,
  "profilePicture": "/uploads/profile-pictures/..."
}
```

### Search Result Structure
```json
{
  "items": [/* service workers */],
  "totalCount": 150,
  "page": 1,
  "pageSize": 10,
  "totalPages": 15,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "usedFallbackLocation": false,
  "searchLatitude": 51.5074,
  "searchLongitude": -0.1278,
  "radiusKm": 25.0
}
```

## 🧪 Testing

### HTTP Test Files
Use the provided `.http` files in the API project:
- `SafeHabour.API.http` - General API tests
- `ServiceWorkers.http` - Service worker specific tests

### Example Tests
```http
### Search Service Workers
POST https://localhost:7001/api/serviceworkers/search
Content-Type: application/json

{
  "page": 1,
  "pageSize": 10,
  "latitude": 51.5074,
  "longitude": -0.1278,
  "radiusKm": 25.0
}
```

## 📝 Logging

### Structured Logging with Serilog
- **Console Logging**: Development debugging
- **File Logging**: Rolling daily logs with 30-day retention
- **Database Logging**: Structured logs in SQL Server
- **Request Tracking**: Comprehensive API request logging

### Log Locations
- **Files**: `wwwroot/logs/log-YYYY-MM-DD.txt`
- **Database**: `Logs` table in main database
- **Console**: Real-time development logs

## 🔄 Development Workflow

### Building the Project
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build SafeHabour.API

# Build for production
dotnet build --configuration Release
```

### Running Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Package Management
```bash
# Add package
dotnet add package PackageName

# Restore packages
dotnet restore

# Update packages
dotnet list package --outdated
```

## 🌍 Deployment

### Production Configuration
1. Update connection strings for production database
2. Configure SendGrid for email services
3. Set up proper JWT secrets
4. Configure CORS for production domains
5. Enable HTTPS enforcement

### Environment Variables
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=ProductionConnectionString
JwtSettings__SecretKey=YourProductionSecret
SendGrid__ApiKey=YourSendGridKey
```

## 🤝 Contributing

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/amazing-feature`
3. **Commit changes**: `git commit -m 'Add amazing feature'`
4. **Push to branch**: `git push origin feature/amazing-feature`
5. **Open a Pull Request**

### Code Standards
- Follow C# naming conventions
- Add XML documentation for public APIs
- Include unit tests for new features
- Maintain clean architecture principles

## 📋 Dependencies

### Core Packages
- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core** - ORM
- **Serilog** - Logging framework
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation

### Authentication & Security
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT authentication
- **Microsoft.AspNetCore.Identity** - User management
- **BCrypt.Net** - Password hashing

### Communication & Email
- **Microsoft.AspNetCore.SignalR** - Real-time communication
- **SendGrid** - Email delivery service

### Development Tools
- **Swashbuckle.AspNetCore** - API documentation
- **Microsoft.Extensions.Logging** - Logging abstractions

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Check the documentation in the `/docs` folder
- Review the API documentation at `/swagger`

## 🎯 Roadmap

### Upcoming Features
- [ ] Real-time availability status
- [ ] Advanced rating and review system
- [ ] Payment integration
- [ ] Mobile app API support
- [ ] Multi-language support
- [ ] Advanced analytics dashboard

### Performance Improvements
- [ ] Redis caching implementation
- [ ] Database query optimization
- [ ] API rate limiting
- [ ] Response compression

---

**Built with ❤️ using ASP.NET Core 8.0**
