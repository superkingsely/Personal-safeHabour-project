using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SafeHabour.Application.Interfaces;
using SafeHabour.Data.Entities;
using SafeHabour.Infrastructure.Interfaces;
using SafeHabour.Models.Response;
using SafeHabour.Models.Requests;

namespace SafeHabour.Application.Managers;

public class ServiceWorkerService : IServiceWorkerService
{
    private readonly IServiceWorkerRepository _serviceWorkerRepository;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ServiceWorkerService> _logger;

    public ServiceWorkerService(
        IServiceWorkerRepository serviceWorkerRepository,
        UserManager<User> userManager,
        ILogger<ServiceWorkerService> logger)
    {
        _serviceWorkerRepository = serviceWorkerRepository;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Searches for service workers with pagination and location-based filtering
    /// </summary>
    /// <param name="searchRequest">Search parameters</param>
    /// <param name="currentUserId">Current user ID for location fallback (optional)</param>
    /// <returns>ServiceResult containing paginated search results</returns>
    public async Task<ServiceResult<PaginatedResponse<ServiceWorkerSearchResultDto>>> SearchServiceWorkersAsync(
        SearchServiceWorkersRequest searchRequest, 
        Guid? currentUserId = null)
    {
        try
        {
            _logger.LogInformation("Executing service worker search with parameters: Page={Page}, PageSize={PageSize}, SearchTerm={SearchTerm}, Latitude={Latitude}, Longitude={Longitude}, RadiusKm={RadiusKm}, CurrentUserId={CurrentUserId}",
                searchRequest.Page, searchRequest.PageSize, searchRequest.SearchTerm, 
                searchRequest.Latitude, searchRequest.Longitude, searchRequest.RadiusKm, currentUserId);

            // Validate search request
            var validationResult = ValidateSearchRequest(searchRequest);
            if (!validationResult.Success)
            {
                _logger.LogWarning("Search request validation failed: {Message}", validationResult.Message);
                return ServiceResult<PaginatedResponse<ServiceWorkerSearchResultDto>>.FailureResult(validationResult.Message);
            }

            // Validate current user if provided (for location fallback)
            if (currentUserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(currentUserId.Value.ToString());
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Invalid or inactive user provided for location fallback: {UserId}", currentUserId);
                    // Don't fail the search, just log and continue without fallback
                    currentUserId = null;
                }
                else
                {
                    _logger.LogDebug("Validated user for location fallback: {UserId}", currentUserId);
                }
            }

            // Execute search through repository
            var searchResult = await _serviceWorkerRepository.SearchServiceWorkersAsync(searchRequest, currentUserId);

            if (searchResult == null)
            {
                _logger.LogWarning("Search returned null result");
                return ServiceResult<PaginatedResponse<ServiceWorkerSearchResultDto>>.FailureResult("Search failed to execute");
            }

            _logger.LogInformation("Service worker search completed successfully: Found {TotalCount} results, Page {Page}/{TotalPages}, UsedFallbackLocation={UsedFallback}",
                searchResult.TotalCount, searchResult.Page, searchResult.TotalPages, searchResult.UsedFallbackLocation);

            // Additional business logic validation if needed
            // For example, filtering out inactive service workers, applying user-specific permissions, etc.

            return ServiceResult<PaginatedResponse<ServiceWorkerSearchResultDto>>.SuccessResult(
                searchResult, 
                $"Found {searchResult.TotalCount} service workers matching your criteria");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching service workers with parameters: Page={Page}, PageSize={PageSize}, SearchTerm={SearchTerm}",
                searchRequest.Page, searchRequest.PageSize, searchRequest.SearchTerm);
            
            return ServiceResult<PaginatedResponse<ServiceWorkerSearchResultDto>>.FailureResult(
                "An error occurred while searching for service workers", 
                ex.Message);
        }
    }

    /// <summary>
    /// Validates the search request parameters
    /// </summary>
    /// <param name="request">The search request to validate</param>
    /// <returns>Validation result</returns>
    private ServiceResult ValidateSearchRequest(SearchServiceWorkersRequest request)
    {
        // Validate pagination parameters
        if (request.Page < 1)
        {
            return ServiceResult.FailureResult("Page number must be greater than 0");
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return ServiceResult.FailureResult("Page size must be between 1 and 100");
        }

        // Validate location parameters
        if (request.Latitude.HasValue || request.Longitude.HasValue)
        {
            if (!request.Latitude.HasValue || !request.Longitude.HasValue)
            {
                return ServiceResult.FailureResult("Both latitude and longitude must be provided for location-based search");
            }

            if (request.Latitude < -90 || request.Latitude > 90)
            {
                return ServiceResult.FailureResult("Latitude must be between -90 and 90 degrees");
            }

            if (request.Longitude < -180 || request.Longitude > 180)
            {
                return ServiceResult.FailureResult("Longitude must be between -180 and 180 degrees");
            }
        }

        // Validate radius
        if (request.RadiusKm <= 0)
        {
            return ServiceResult.FailureResult("Radius must be greater than 0 kilometers");
        }

        // Validate hourly rate range
        if (request.MinHourlyRate.HasValue && request.MinHourlyRate < 0)
        {
            return ServiceResult.FailureResult("Minimum hourly rate cannot be negative");
        }

        if (request.MaxHourlyRate.HasValue && request.MaxHourlyRate < 0)
        {
            return ServiceResult.FailureResult("Maximum hourly rate cannot be negative");
        }

        if (request.MinHourlyRate.HasValue && request.MaxHourlyRate.HasValue && 
            request.MinHourlyRate > request.MaxHourlyRate)
        {
            return ServiceResult.FailureResult("Minimum hourly rate cannot be greater than maximum hourly rate");
        }

        // Validate search term length
        if (!string.IsNullOrEmpty(request.SearchTerm) && request.SearchTerm.Length > 500)
        {
            return ServiceResult.FailureResult("Search term cannot exceed 500 characters");
        }

        return ServiceResult.SuccessResult("Validation passed");
    }

    /// <summary>
    /// Gets service worker details by user ID with business logic validation
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>ServiceResult containing ServiceWorkerDto if found and accessible</returns>
    public async Task<ServiceResult<ServiceWorkerDto>> GetServiceWorkerByUserIdAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Retrieving service worker details for user ID: {UserId}", userId);

            // Validate user exists and is active
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return ServiceResult<ServiceWorkerDto>.FailureResult("User not found");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Attempted to access inactive user: {UserId}", userId);
                return ServiceResult<ServiceWorkerDto>.FailureResult("User account is inactive");
            }

            // Get service worker details
            var serviceWorker = await _serviceWorkerRepository.GetServiceWorkerByUserIdAsync(userId);
            if (serviceWorker == null)
            {
                _logger.LogWarning("Service worker profile not found for user: {UserId}", userId);
                return ServiceResult<ServiceWorkerDto>.FailureResult("Service worker profile not found");
            }

            _logger.LogInformation("Successfully retrieved service worker details for user: {UserId}", userId);
            return ServiceResult<ServiceWorkerDto>.SuccessResult(serviceWorker, "Service worker details retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service worker for user ID: {UserId}", userId);
            return ServiceResult<ServiceWorkerDto>.FailureResult(
                "An error occurred while retrieving service worker details", 
                ex.Message);
        }
    }

    /// <summary>
    /// Gets service worker details by service worker ID with business logic validation
    /// </summary>
    /// <param name="serviceWorkerId">The service worker ID</param>
    /// <returns>ServiceResult containing ServiceWorkerDto if found and accessible</returns>
    public async Task<ServiceResult<ServiceWorkerDto>> GetServiceWorkerByIdAsync(int serviceWorkerId)
    {
        try
        {
            _logger.LogInformation("Retrieving service worker details for service worker ID: {ServiceWorkerId}", serviceWorkerId);

            // Get service worker details
            var serviceWorker = await _serviceWorkerRepository.GetServiceWorkerByIdAsync(serviceWorkerId);
            if (serviceWorker == null)
            {
                _logger.LogWarning("Service worker not found: {ServiceWorkerId}", serviceWorkerId);
                return ServiceResult<ServiceWorkerDto>.FailureResult("Service worker not found");
            }

            // Additional validation can be added here if needed
            // For example, checking if the associated user is active

            _logger.LogInformation("Successfully retrieved service worker details for service worker ID: {ServiceWorkerId}", serviceWorkerId);
            return ServiceResult<ServiceWorkerDto>.SuccessResult(serviceWorker, "Service worker details retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service worker for service worker ID: {ServiceWorkerId}", serviceWorkerId);
            return ServiceResult<ServiceWorkerDto>.FailureResult(
                "An error occurred while retrieving service worker details", 
                ex.Message);
        }
    }

    /// <summary>
    /// Gets service worker profile completion status
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>ServiceResult containing profile completion information</returns>
    public async Task<ServiceResult<ServiceWorkerProfileStatus>> GetProfileCompletionStatusAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Checking profile completion for service worker user: {UserId}", userId);

            var result = new ServiceWorkerProfileStatus();
            var missingFields = new List<string>();
            var completedFields = new List<string>();

            // Get user and service worker details
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var serviceWorker = await _serviceWorkerRepository.GetServiceWorkerByUserIdAsync(userId);

            if (user == null)
            {
                return ServiceResult<ServiceWorkerProfileStatus>.FailureResult("User not found");
            }

            // Check basic user fields
            if (string.IsNullOrWhiteSpace(user.FirstName))
                missingFields.Add("First Name");
            else
                completedFields.Add("First Name");

            if (string.IsNullOrWhiteSpace(user.LastName))
                missingFields.Add("Last Name");
            else
                completedFields.Add("Last Name");

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                missingFields.Add("Phone Number");
            else
                completedFields.Add("Phone Number");

            if (!user.EmailConfirmed)
                missingFields.Add("Email Verification");
            else
                completedFields.Add("Email Verification");

            // Check service worker specific fields
            if (serviceWorker == null)
            {
                missingFields.Add("Service Worker Profile");
            }
            else
            {
                completedFields.Add("Service Worker Profile");

                if (string.IsNullOrWhiteSpace(serviceWorker.StreetAddress))
                    missingFields.Add("Street Address");
                else
                    completedFields.Add("Street Address");

                if (string.IsNullOrWhiteSpace(serviceWorker.City))
                    missingFields.Add("City");
                else
                    completedFields.Add("City");

                if (string.IsNullOrWhiteSpace(serviceWorker.PostalCode))
                    missingFields.Add("Postal Code");
                else
                    completedFields.Add("Postal Code");

                if (string.IsNullOrWhiteSpace(serviceWorker.Country))
                    missingFields.Add("Country");
                else
                    completedFields.Add("Country");

                if (serviceWorker.Services == null || !serviceWorker.Services.Any())
                    missingFields.Add("Services Offered");
                else
                    completedFields.Add("Services Offered");

                if (serviceWorker.Languages == null || !serviceWorker.Languages.Any())
                    missingFields.Add("Languages");
                else
                    completedFields.Add("Languages");

                if (serviceWorker.HourlyRate <= 0)
                    missingFields.Add("Hourly Rate");
                else
                    completedFields.Add("Hourly Rate");

                if (string.IsNullOrWhiteSpace(serviceWorker.Bio))
                    missingFields.Add("Bio");
                else
                    completedFields.Add("Bio");
            }

            // Calculate completion percentage
            var totalFields = missingFields.Count + completedFields.Count;
            result.CompletionPercentage = totalFields > 0 ? (double)completedFields.Count / totalFields * 100 : 0;
            result.IsComplete = missingFields.Count == 0;
            result.MissingFields = missingFields;
            result.CompletedFields = completedFields;
            result.Message = result.IsComplete ? "Profile is complete" : $"{missingFields.Count} fields need to be completed";

            _logger.LogInformation("Profile completion calculated for service worker user {UserId}: {Percentage}%", 
                userId, result.CompletionPercentage);

            return ServiceResult<ServiceWorkerProfileStatus>.SuccessResult(result, "Profile completion status calculated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking profile completion for service worker user: {UserId}", userId);
            return ServiceResult<ServiceWorkerProfileStatus>.FailureResult(
                "An error occurred while checking profile completion", 
                ex.Message);
        }
    }

    /// <summary>
    /// Updates a service worker profile with file upload handling
    /// </summary>
    /// <param name="request">The update service worker request</param>
    /// <returns>ServiceResult containing updated ServiceWorkerDto if successful</returns>
    public async Task<ServiceResult<ServiceWorkerDto>> UpdateServiceWorkerAsync(UpdateServiceWorkerRequest request)
    {
        try
        {
            _logger.LogInformation("Updating service worker profile for user ID: {UserId}", request.UserId);

            // Validate user exists and is active
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found during update: {UserId}", request.UserId);
                return ServiceResult<ServiceWorkerDto>.FailureResult("User not found");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Attempted to update inactive user: {UserId}", request.UserId);
                return ServiceResult<ServiceWorkerDto>.FailureResult("User account is inactive");
            }

            string? profilePicturePath = null;

            // Handle profile picture upload if provided
            if (request.ProfilePicture != null && request.ProfilePicture.Length > 0)
            {
                _logger.LogInformation("Processing profile picture upload for service worker user: {UserId}", request.UserId);

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(request.ProfilePicture.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning("Invalid file type uploaded: {FileName}", request.ProfilePicture.FileName);
                    return ServiceResult<ServiceWorkerDto>.FailureResult(
                        "Invalid file type. Only JPG, PNG, and GIF files are allowed.");
                }

                // Validate file size (5MB limit)
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (request.ProfilePicture.Length > maxFileSize)
                {
                    _logger.LogWarning("File size too large: {Size} bytes", request.ProfilePicture.Length);
                    return ServiceResult<ServiceWorkerDto>.FailureResult(
                        "File size exceeds the maximum limit of 5MB.");
                }

                try
                {
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

                    _logger.LogInformation("Profile picture saved successfully: {FilePath}", profilePicturePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving profile picture for service worker user: {UserId}", request.UserId);
                    return ServiceResult<ServiceWorkerDto>.FailureResult(
                        "Failed to save profile picture. Please try again.");
                }
            }

            // Call repository to update service worker
            var result = await _serviceWorkerRepository.UpdateServiceWorkerAsync(request, profilePicturePath);

            if (result != null)
            {
                _logger.LogInformation("Successfully updated service worker profile for user: {UserId}", request.UserId);
                return ServiceResult<ServiceWorkerDto>.SuccessResult(result, "Service worker profile updated successfully");
            }

            _logger.LogWarning("Failed to update service worker profile for user: {UserId}", request.UserId);
            return ServiceResult<ServiceWorkerDto>.FailureResult("Failed to update service worker profile");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service worker profile for user: {UserId}", request.UserId);
            return ServiceResult<ServiceWorkerDto>.FailureResult(
                "An error occurred while updating the profile", 
                ex.Message);
        }
    }
}
