using Microsoft.EntityFrameworkCore;
using SafeHabour.Data;
using SafeHabour.Data.Data;
using SafeHabour.Data.Entities;
using SafeHabour.Infrastructure.Interfaces;
using SafeHabour.Models.Response;
using SafeHabour.Models.Requests;
using SafeHabour.Data.DTOMapper.ServiceWorker;
using System.Text.Json;

namespace SafeHabour.Infrastructure.Repositories;

public class ServiceWorkerRepository : IServiceWorkerRepository
{
    private readonly ApiDbContext _context;

    public ServiceWorkerRepository(ApiDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets service worker details by user ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="baseUrl">Base URL for constructing complete profile picture URLs (optional)</param>
    /// <returns>ServiceWorkerDto if found, null otherwise</returns>
    public async Task<ServiceWorkerDto?> GetServiceWorkerByUserIdAsync(Guid userId, string? baseUrl = null)
    {
        try
        {
            var serviceWorkerUser = await _context.ServiceWorkerUsers
                .Include(sw => sw.User)
                .FirstOrDefaultAsync(sw => sw.UserId == userId);

            if (serviceWorkerUser == null)
                return null;

            return ServiceWorkerMapper.ToDtoWithUserDetails(serviceWorkerUser, baseUrl);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets service worker details by service worker ID
    /// </summary>
    /// <param name="serviceWorkerId">The service worker ID</param>
    /// <param name="baseUrl">Base URL for constructing complete profile picture URLs (optional)</param>
    /// <returns>ServiceWorkerDto if found, null otherwise</returns>
    public async Task<ServiceWorkerDto?> GetServiceWorkerByIdAsync(int serviceWorkerId, string? baseUrl = null)
    {
        try
        {
            var serviceWorkerUser = await _context.ServiceWorkerUsers
                .Include(sw => sw.User)
                .FirstOrDefaultAsync(sw => sw.Id == serviceWorkerId);

            if (serviceWorkerUser == null)
                return null;

            return ServiceWorkerMapper.ToDtoWithUserDetails(serviceWorkerUser, baseUrl);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Updates a service worker
    /// </summary>
    /// <param name="request">The update service worker request</param>
    /// <param name="profilePicturePath">The relative path to the uploaded profile picture (optional)</param>
    /// <returns>Updated ServiceWorkerDto if successful, null otherwise</returns>
    public async Task<ServiceWorkerDto?> UpdateServiceWorkerAsync(UpdateServiceWorkerRequest request, string? profilePicturePath = null, string? baseUrl = null)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out var userId))
                return null;

            var serviceWorkerUser = await _context.ServiceWorkerUsers
                .Include(sw => sw.User)
                .FirstOrDefaultAsync(sw => sw.UserId == userId);

            if (serviceWorkerUser == null)
                return null;

            var user = serviceWorkerUser.User;

            // Update user fields if provided
            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            if (request.DateOfBirth.HasValue)
                user.DateOfBirth = request.DateOfBirth.Value;

            if (!string.IsNullOrEmpty(request.Gender))
                user.Gender = request.Gender;

            if (!string.IsNullOrEmpty(request.Bio))
                user.Bio = request.Bio;

            if (!string.IsNullOrEmpty(profilePicturePath))
                user.ProfilePicturePath = profilePicturePath;

            if (!string.IsNullOrEmpty(request.StreetAddress))
                user.StreetAddress = request.StreetAddress;

            if (!string.IsNullOrEmpty(request.City))
                user.City = request.City;

            if (!string.IsNullOrEmpty(request.Country))
                user.Country = request.Country;

            if (!string.IsNullOrEmpty(request.PostalCode))
                user.PostalCode = request.PostalCode;

            // Update coordinates if provided
            if (request.Latitude.HasValue)
                user.Latitude = request.Latitude.Value;

            if (request.Longitude.HasValue)
                user.Longitude = request.Longitude.Value;

            // Update service worker specific fields if provided
            if (request.Services != null && request.Services.Any())
                serviceWorkerUser.Services = request.Services;

            if (request.Languages != null && request.Languages.Any())
                serviceWorkerUser.Languages = request.Languages;

            if (request.HourlyRate.HasValue)
                serviceWorkerUser.HourlyRate = request.HourlyRate.Value;

            // Update timestamps
            user.UpdatedAt = DateTime.UtcNow;
            serviceWorkerUser.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            _context.ServiceWorkerUsers.Update(serviceWorkerUser);
            await _context.SaveChangesAsync();

            return ServiceWorkerMapper.ToDtoWithUserDetails(serviceWorkerUser, baseUrl);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Searches for service workers with pagination and location-based filtering
    /// </summary>
    /// <param name="searchRequest">Search parameters</param>
    /// <param name="currentUserId">Current user ID for fallback location</param>
    /// <param name="baseUrl">Base URL for constructing complete profile picture URLs (optional)</param>
    /// <returns>Paginated list of service workers</returns>
    public async Task<PaginatedResponse<ServiceWorkerSearchResultDto>> SearchServiceWorkersAsync(SearchServiceWorkersRequest searchRequest, Guid? currentUserId = null, string? baseUrl = null)
    {
        try
        {
            // Get search coordinates (either from request or fallback to user location)
            var (searchLat, searchLng, usedFallback, fallbackSource) = await GetSearchCoordinatesAsync(searchRequest, currentUserId);

            // Build the base query
            var query = _context.ServiceWorkerUsers
                .Include(sw => sw.User)
                .Where(sw => sw.User != null);

            // Apply text search filters
            if (!string.IsNullOrEmpty(searchRequest.SearchTerm))
            {
                var searchTerm = searchRequest.SearchTerm.ToLower();
                query = query.Where(sw => 
                    (sw.User.FirstName + " " + sw.User.LastName).ToLower().Contains(searchTerm) ||
                    (sw.User.Bio != null && sw.User.Bio.ToLower().Contains(searchTerm)) ||
                    sw.ServicesJson.ToLower().Contains(searchTerm));
            }

            // Apply service names filter (multiple services)
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

            // Apply languages filter (multiple languages)
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

            // Apply hourly rate filters
            if (searchRequest.MinHourlyRate.HasValue)
            {
                query = query.Where(sw => sw.HourlyRate >= searchRequest.MinHourlyRate.Value);
            }

            if (searchRequest.MaxHourlyRate.HasValue)
            {
                query = query.Where(sw => sw.HourlyRate <= searchRequest.MaxHourlyRate.Value);
            }

            // Apply verified filter
            if (searchRequest.VerifiedOnly == true)
            {
                query = query.Where(sw => sw.User.EmailConfirmed == true);
            }

            // Get all matching service workers for distance calculation
            var allServiceWorkers = await query.ToListAsync();

            // Calculate distances and filter by radius
            var serviceWorkersWithDistance = new List<(ServiceWorkerUser serviceWorker, double distance)>();

            foreach (var sw in allServiceWorkers)
            {
                var swLat = sw.User.Latitude ?? sw.Latitude;
                var swLng = sw.User.Longitude ?? sw.Longitude;

                if (swLat.HasValue && swLng.HasValue && searchLat.HasValue && searchLng.HasValue)
                {
                    var distance = CalculateDistance(searchLat.Value, searchLng.Value, swLat.Value, swLng.Value);
                    if (distance <= searchRequest.RadiusKm)
                    {
                        serviceWorkersWithDistance.Add((sw, distance));
                    }
                }
                else
                {
                    // Include service workers without location data but with a high distance value for sorting
                    serviceWorkersWithDistance.Add((sw, 9999));
                }
            }

            // Apply sorting
            serviceWorkersWithDistance = ApplySorting(serviceWorkersWithDistance, searchRequest.SortBy, searchRequest.SortDirection);

            // Apply pagination
            var totalCount = serviceWorkersWithDistance.Count;
            var paginatedItems = serviceWorkersWithDistance
                .Skip((searchRequest.Page - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize)
                .ToList();

            // Convert to DTOs
            var resultItems = paginatedItems.Select(item => 
                ServiceWorkerMapper.ToSearchResultDto(item.serviceWorker, item.distance, baseUrl)).ToList();

            return new PaginatedResponse<ServiceWorkerSearchResultDto>
            {
                Items = resultItems,
                TotalCount = totalCount,
                Page = searchRequest.Page,
                PageSize = searchRequest.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize),
                HasNextPage = searchRequest.Page * searchRequest.PageSize < totalCount,
                HasPreviousPage = searchRequest.Page > 1,
                SearchLatitude = searchLat,
                SearchLongitude = searchLng,
                RadiusKm = searchRequest.RadiusKm,
                UsedFallbackLocation = usedFallback,
                FallbackLocationSource = fallbackSource
            };
        }
        catch (Exception)
        {
            // Return empty result on error
            return new PaginatedResponse<ServiceWorkerSearchResultDto>
            {
                Items = new List<ServiceWorkerSearchResultDto>(),
                TotalCount = 0,
                Page = searchRequest.Page,
                PageSize = searchRequest.PageSize,
                TotalPages = 0,
                HasNextPage = false,
                HasPreviousPage = false
            };
        }
    }

    /// <summary>
    /// Gets search coordinates from request or fallback to user location
    /// </summary>
    private async Task<(double? lat, double? lng, bool usedFallback, string? fallbackSource)> GetSearchCoordinatesAsync(
        SearchServiceWorkersRequest searchRequest, Guid? currentUserId)
    {
        // First, try to use coordinates from the search request
        if (searchRequest.Latitude.HasValue && searchRequest.Longitude.HasValue)
        {
            return (searchRequest.Latitude, searchRequest.Longitude, false, null);
        }

        // Fallback to current user's location if available
        if (currentUserId.HasValue)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId.Value);
            if (user?.Latitude.HasValue == true && user?.Longitude.HasValue == true)
            {
                return (user.Latitude, user.Longitude, true, "User Profile Location");
            }
        }

        // No location available - will show all results without distance filtering
        return (null, null, true, "No Location Available");
    }

    /// <summary>
    /// Calculates the distance between two points using the Haversine formula
    /// </summary>
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadius = 6371; // Earth's radius in kilometers

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = earthRadius * c;

        return distance;
    }

    /// <summary>
    /// Converts degrees to radians
    /// </summary>
    private static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    /// <summary>
    /// Applies sorting to the service workers list
    /// </summary>
    private static List<(ServiceWorkerUser serviceWorker, double distance)> ApplySorting(
        List<(ServiceWorkerUser serviceWorker, double distance)> items,
        ServiceWorkerSortBy sortBy,
        SortDirection sortDirection)
    {
        var query = items.AsEnumerable();

        query = sortBy switch
        {
            ServiceWorkerSortBy.Distance => sortDirection == SortDirection.Ascending
                ? query.OrderBy(x => x.distance)
                : query.OrderByDescending(x => x.distance),

            ServiceWorkerSortBy.HourlyRate => sortDirection == SortDirection.Ascending
                ? query.OrderBy(x => x.serviceWorker.HourlyRate)
                : query.OrderByDescending(x => x.serviceWorker.HourlyRate),

            ServiceWorkerSortBy.Name => sortDirection == SortDirection.Ascending
                ? query.OrderBy(x => x.serviceWorker.User.FirstName).ThenBy(x => x.serviceWorker.User.LastName)
                : query.OrderByDescending(x => x.serviceWorker.User.FirstName).ThenByDescending(x => x.serviceWorker.User.LastName),

            ServiceWorkerSortBy.CreatedDate => sortDirection == SortDirection.Ascending
                ? query.OrderBy(x => x.serviceWorker.CreatedAt)
                : query.OrderByDescending(x => x.serviceWorker.CreatedAt),

            ServiceWorkerSortBy.Rating => sortDirection == SortDirection.Ascending
                ? query.OrderBy(x => 0) // TODO: Implement rating when review system is added
                : query.OrderByDescending(x => 0),

            _ => query.OrderBy(x => x.distance)
        };

        return query.ToList();
    }
}
