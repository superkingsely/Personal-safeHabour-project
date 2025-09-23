using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SafeHabour.Application.Interfaces;
using SafeHabour.Data.Entities;
using SafeHabour.Infrastructure.Interfaces;
using SafeHabour.Models.Response;
using SafeHabour.Models.Requests;

namespace SafeHabour.Application.Managers;

public class ClientUserService : IClientUserService
{
    private readonly IClientUserRepository _clientUserRepository;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ClientUserService> _logger;

    public ClientUserService(
        IClientUserRepository clientUserRepository,
        UserManager<User> userManager,
        ILogger<ClientUserService> logger)
    {
        _clientUserRepository = clientUserRepository;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Gets client user details by user ID with business logic validation
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>ServiceResult containing ClientUserDto if found and accessible</returns>
    public async Task<ServiceResult<ClientUserDto>> GetClientUserByUserIdAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Retrieving client user details for user ID: {UserId}", userId);

            // Validate user exists and is active
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return ServiceResult<ClientUserDto>.FailureResult("User not found");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Attempted to access inactive user: {UserId}", userId);
                return ServiceResult<ClientUserDto>.FailureResult("User account is inactive");
            }

            // Get client user details
            var clientUser = await _clientUserRepository.GetClientUserByUserIdAsync(userId);
            if (clientUser == null)
            {
                _logger.LogWarning("Client user profile not found for user: {UserId}", userId);
                return ServiceResult<ClientUserDto>.FailureResult("Client user profile not found");
            }

            _logger.LogInformation("Successfully retrieved client user details for user: {UserId}", userId);
            return ServiceResult<ClientUserDto>.SuccessResult(clientUser, "Client user details retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving client user for user ID: {UserId}", userId);
            return ServiceResult<ClientUserDto>.FailureResult(
                "An error occurred while retrieving client user details", 
                ex.Message);
        }
    }

    /// <summary>
    /// Gets client user details by client user ID with business logic validation
    /// </summary>
    /// <param name="clientUserId">The client user ID</param>
    /// <returns>ServiceResult containing ClientUserDto if found and accessible</returns>
    public async Task<ServiceResult<ClientUserDto>> GetClientUserByIdAsync(int clientUserId)
    {
        try
        {
            _logger.LogInformation("Retrieving client user details for client user ID: {ClientUserId}", clientUserId);

            // Get client user details
            var clientUser = await _clientUserRepository.GetClientUserByIdAsync(clientUserId);
            if (clientUser == null)
            {
                _logger.LogWarning("Client user not found: {ClientUserId}", clientUserId);
                return ServiceResult<ClientUserDto>.FailureResult("Client user not found");
            }

            // Additional validation can be added here if needed
            // For example, checking if the associated user is active

            _logger.LogInformation("Successfully retrieved client user details for client user ID: {ClientUserId}", clientUserId);
            return ServiceResult<ClientUserDto>.SuccessResult(clientUser, "Client user details retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving client user for client user ID: {ClientUserId}", clientUserId);
            return ServiceResult<ClientUserDto>.FailureResult(
                "An error occurred while retrieving client user details", 
                ex.Message);
        }
    }

    /// <summary>
    /// Gets client user profile completion status
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>ServiceResult containing profile completion information</returns>
    public async Task<ServiceResult<ClientUserProfileStatus>> GetProfileCompletionStatusAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Checking profile completion for user: {UserId}", userId);

            var result = new ClientUserProfileStatus();
            var missingFields = new List<string>();
            var completedFields = new List<string>();

            // Get user and client user details
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var clientUser = await _clientUserRepository.GetClientUserByUserIdAsync(userId);

            if (user == null)
            {
                return ServiceResult<ClientUserProfileStatus>.FailureResult("User not found");
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

            // Check client user specific fields
            if (clientUser == null)
            {
                missingFields.Add("Client Profile");
            }
            else
            {
                completedFields.Add("Client Profile");

                if (string.IsNullOrWhiteSpace(clientUser.StreetAddress))
                    missingFields.Add("Street Address");
                else
                    completedFields.Add("Street Address");

                if (string.IsNullOrWhiteSpace(clientUser.City))
                    missingFields.Add("City");
                else
                    completedFields.Add("City");

                if (string.IsNullOrWhiteSpace(clientUser.PostalCode))
                    missingFields.Add("Postal Code");
                else
                    completedFields.Add("Postal Code");

                if (string.IsNullOrWhiteSpace(clientUser.Country))
                    missingFields.Add("Country");
                else
                    completedFields.Add("Country");
            }

            // Calculate completion percentage
            var totalFields = missingFields.Count + completedFields.Count;
            result.CompletionPercentage = totalFields > 0 ? (double)completedFields.Count / totalFields * 100 : 0;
            result.IsComplete = missingFields.Count == 0;
            result.MissingFields = missingFields;
            result.CompletedFields = completedFields;
            result.Message = result.IsComplete ? "Profile is complete" : $"{missingFields.Count} fields need to be completed";

            _logger.LogInformation("Profile completion calculated for user {UserId}: {Percentage}%", 
                userId, result.CompletionPercentage);

            return ServiceResult<ClientUserProfileStatus>.SuccessResult(result, "Profile completion status calculated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking profile completion for user: {UserId}", userId);
            return ServiceResult<ClientUserProfileStatus>.FailureResult(
                "An error occurred while checking profile completion", 
                ex.Message);
        }
    }

    /// <summary>
    /// Updates a client user profile with file upload handling
    /// </summary>
    /// <param name="request">The update client user request</param>
    /// <returns>ServiceResult containing updated ClientUserDto if successful</returns>
    public async Task<ServiceResult<ClientUserDto>> UpdateClientUserAsync(UpdateClientUserRequest request)
    {
        try
        {
            _logger.LogInformation("Updating client user profile for user ID: {UserId}", request.UserId);

            // Validate user exists and is active
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found during update: {UserId}", request.UserId);
                return ServiceResult<ClientUserDto>.FailureResult("User not found");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Attempted to update inactive user: {UserId}", request.UserId);
                return ServiceResult<ClientUserDto>.FailureResult("User account is inactive");
            }

            string? profilePicturePath = null;

            // Handle profile picture upload if provided
            if (request.ProfilePicture != null && request.ProfilePicture.Length > 0)
            {
                _logger.LogInformation("Processing profile picture upload for user: {UserId}", request.UserId);

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(request.ProfilePicture.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning("Invalid file type uploaded: {FileName}", request.ProfilePicture.FileName);
                    return ServiceResult<ClientUserDto>.FailureResult(
                        "Invalid file type. Only JPG, PNG, and GIF files are allowed.");
                }

                // Validate file size (5MB limit)
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (request.ProfilePicture.Length > maxFileSize)
                {
                    _logger.LogWarning("File size too large: {Size} bytes", request.ProfilePicture.Length);
                    return ServiceResult<ClientUserDto>.FailureResult(
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
                    _logger.LogError(ex, "Error saving profile picture for user: {UserId}", request.UserId);
                    return ServiceResult<ClientUserDto>.FailureResult(
                        "Failed to save profile picture. Please try again.");
                }
            }

            // Call repository to update client user
            var result = await _clientUserRepository.UpdateClientUserAsync(request, profilePicturePath);

            if (result != null)
            {
                _logger.LogInformation("Successfully updated client user profile for user: {UserId}", request.UserId);
                return ServiceResult<ClientUserDto>.SuccessResult(result, "Client user profile updated successfully");
            }

            _logger.LogWarning("Failed to update client user profile for user: {UserId}", request.UserId);
            return ServiceResult<ClientUserDto>.FailureResult("Failed to update client user profile");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating client user profile for user: {UserId}", request.UserId);
            return ServiceResult<ClientUserDto>.FailureResult(
                "An error occurred while updating the profile", 
                ex.Message);
        }
    }
}
