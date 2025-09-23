using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafeHabour.Application.Interfaces;
using SafeHabour.Data;
using SafeHabour.Data.Data;
using SafeHabour.Data.Entities;
using SafeHabour.Models.Enums;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;
using SafeHabour.Data.DTOMapper.User;
using SafeHabour.Data.DTOMapper.NotificationSetting;
using System.Security.Claims;

namespace SafeHabour.Application.Managers;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<UserRole> _roleManager;
    private readonly ApiDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<User> userManager,
        RoleManager<UserRole> roleManager,
        ApiDbContext context,
        IJwtService jwtService,
        IEmailService emailService,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _jwtService = jwtService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ServiceResult<object>> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found for email: {Email}", request.Email);
                return ServiceResult<object>.FailureResult("Invalid email or password");
            }

            var result = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!result)
            {
                _logger.LogWarning("Login failed - invalid password for user: {UserId} ({Email})", user.Id, request.Email);
                return ServiceResult<object>.FailureResult("Invalid email or password");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed - account deactivated for user: {UserId} ({Email})", user.Id, request.Email);
                return ServiceResult<object>.FailureResult("Account is deactivated");
            }

            // Check if email is verified
            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Login failed - email not verified for user: {UserId} ({Email})", user.Id, request.Email);
                return ServiceResult<object>.FailureResult("Please verify your email address before logging in. Check your email for the verification link.", 
                    new List<string> { "EMAIL_NOT_VERIFIED" });
            }

            // Check if 2FA is enabled
            if (user.IsTwoFactorAuthenticationEnabled)
            {
                _logger.LogInformation("2FA required for user: {UserId} ({Email})", user.Id, request.Email);
                
                // Generate and send 2FA code
                var code = GenerateTwoFactorCode();
                var expiresAt = DateTime.UtcNow.AddMinutes(10); // Code expires in 10 minutes

                // Store the code in database
                var twoFactorCode = new TwoFactorAuthCode
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Code = code,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TwoFactorAuthCodes.Add(twoFactorCode);
                await _context.SaveChangesAsync();

                _logger.LogDebug("2FA code generated and stored for user: {UserId}", user.Id);

                // Send email with code
                try
                {
                    await _emailService.SendTwoFactorCodeAsync(user.Email!, code, $"{user.FirstName} {user.LastName}");
                    _logger.LogInformation("2FA code sent via email to user: {UserId}", user.Id);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send 2FA code email to user: {UserId}", user.Id);
                    return ServiceResult<object>.FailureResult("Failed to send verification code. Please try again.");
                }

                return ServiceResult<object>.TwoFactorRequired("Two-factor authentication code sent to your email. Please verify to complete login.");
            }

            // Update last login
            await UpdateLastLoginAsync(user.Id.ToString());

            // Generate token
            var roles = await _userManager.GetRolesAsync(user);
            var token = await _jwtService.GenerateJwtTokenAsync(user, roles);
            var userDto = await UserMapper.ToDtoAsync(user, _userManager);

            _logger.LogInformation("Login successful for user: {UserId} ({Email}) with roles: {Roles}", 
                user.Id, request.Email, string.Join(", ", roles));

            var successResult = ServiceResult<object>.SuccessResult(new
            {
                Token = token.Token,
                TokenExpiry = token.Expiry,
                User = userDto,
                Roles = roles.ToList()
            }, "Login successful");
            
            successResult.User = userDto;
            return successResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed with exception for email: {Email}", request.Email);
            return ServiceResult<object>.FailureResult("Login failed");
        }
    }

    public async Task<ServiceResult<object>> CreateClientUserAsync(CreateClientUserRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _logger.LogInformation("Creating client user account for email: {Email}", request.Email);

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Client user creation failed - email already exists: {Email}", request.Email);
                return ServiceResult<object>.FailureResult("User with this email already exists");
            }

            // Create the user
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                StripeCustomerId = null // Will be set when first payment is made
            };

            _logger.LogDebug("Creating user entity for client: {UserId} ({Email})", user.Id, request.Email);

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("Client user creation failed for email: {Email}. Errors: {Errors}", 
                    request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ServiceResult<object>.FailureResult(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // Add to Client role
            await _userManager.AddToRoleAsync(user, UserType.ClientUser);
            _logger.LogDebug("Added user {UserId} to ClientUser role", user.Id);

            // Create client profile
            var clientUser = new ClientUser
            {
                UserId = user.Id,
                ClientType = request.ClientType,
                CreatedAt = DateTime.UtcNow
            };

            _context.ClientUsers.Add(clientUser);
            await _context.SaveChangesAsync();
            
            _logger.LogDebug("Created client profile for user: {UserId} with type: {ClientType}", user.Id, request.ClientType);

            // Create default notification settings
            await CreateDefaultNotificationSettingsAsync(user.Id);
            _logger.LogDebug("Created default notification settings for user: {UserId}", user.Id);

            await transaction.CommitAsync();
            _logger.LogInformation("Successfully created client user: {UserId} ({Email})", user.Id, request.Email);

            // Generate email confirmation token and send verification email
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            try
            {
                await _emailService.SendEmailConfirmationAsync(user.Email!, emailConfirmationToken, $"{user.FirstName} {user.LastName}", user.Id.ToString());
                _logger.LogInformation("Email confirmation sent to client user: {UserId} ({Email})", user.Id, request.Email);
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Failed to send email confirmation to client user: {UserId} ({Email})", user.Id, request.Email);
                // Don't fail the registration if email sending fails
            }

            var successResult = ServiceResult<object>.SuccessResult(new
            {
                EmailVerificationRequired = true
            }, "User created successfully. Please check your email to verify your account.");
            
            return successResult;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Client user creation failed with exception for email: {Email}", request.Email);
            return ServiceResult<object>.FailureResult("User creation failed");
        }
    }

    public async Task<ServiceResult<object>> CreateServiceWorkerUserAsync(CreateServiceWorkerUserRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _logger.LogInformation("Creating service worker user account for email: {Email}", request.Email);

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Service worker user creation failed - email already exists: {Email}", request.Email);
                return ServiceResult<object>.FailureResult("User with this email already exists");
            }

            // Create the user
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                StreetAddress = request.Address,
                City = request.City,
                PostalCode = request.PostalCode,
                Country = request.Country,
                DateOfBirth = request.DateOfBirth,
                Bio = request.Bio,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                //StripeAccountId = null // Will be set during verification
            };

            _logger.LogDebug("Creating user entity for service worker: {UserId} ({Email}) with {ServiceCount} services", 
                user.Id, request.Email, request.Services.Count());

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("Service worker user creation failed for email: {Email}. Errors: {Errors}", 
                    request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ServiceResult<object>.FailureResult(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // Add to ServiceWorker role
            await _userManager.AddToRoleAsync(user, "ServiceWorker");
            _logger.LogDebug("Added user {UserId} to ServiceWorker role", user.Id);

            // Create service worker profile
            var serviceWorkerUser = new ServiceWorkerUser
            {
                UserId = user.Id,
                Services = request.Services,
                Languages = request.Languages,
                HourlyRate = request.HourlyRate,
                Bio = request.Bio,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceWorkerUsers.Add(serviceWorkerUser);
            await _context.SaveChangesAsync();
            
            _logger.LogDebug("Created service worker profile for user: {UserId} with hourly rate: {HourlyRate}", 
                user.Id, request.HourlyRate);

            // Create default notification settings
            await CreateDefaultNotificationSettingsAsync(user.Id);
            _logger.LogDebug("Created default notification settings for user: {UserId}", user.Id);

            await transaction.CommitAsync();
            _logger.LogInformation("Successfully created service worker user: {UserId} ({Email})", user.Id, request.Email);

            // Generate email confirmation token and send verification email
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            try
            {
                await _emailService.SendEmailConfirmationAsync(user.Email!, emailConfirmationToken, $"{user.FirstName} {user.LastName}", user.Id.ToString());
                _logger.LogInformation("Email confirmation sent to service worker user: {UserId} ({Email})", user.Id, request.Email);
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Failed to send email confirmation to service worker user: {UserId} ({Email})", user.Id, request.Email);
                // Don't fail the registration if email sending fails
            }

            var successResult = ServiceResult<object>.SuccessResult(new
            {
                EmailVerificationRequired = true
            }, "Service worker account created successfully. Please check your email to verify your account.");
            
            return successResult;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Service worker user creation failed with exception for email: {Email}", request.Email);
            return ServiceResult<object>.FailureResult("Service worker creation failed");
        }
    }

    public async Task<ServiceResult<object>> LogoutAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Logout attempt for user: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Logout failed - user not found: {UserId}", userId);
                return ServiceResult<object>.FailureResult("User not found");
            }

            // Update last activity
            user.LastLoginAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Logout successful for user: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Failed to update last login time during logout for user: {UserId}", userId);
            }

            return ServiceResult<object>.SuccessResult(new { }, "Logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed with exception for user: {UserId}", userId);
            return ServiceResult<object>.FailureResult("Logout failed");
        }
    }

    public async Task<ServiceResult<object>> RefreshTokenAsync(string token)
    {
        try
        {
            _logger.LogDebug("Token refresh attempt");

            var isValid = await _jwtService.ValidateTokenAsync(token);
            if (!isValid)
            {
                _logger.LogWarning("Token refresh failed - invalid token");
                return ServiceResult<object>.FailureResult("Invalid token");
            }

            var userGuidId = await _jwtService.GetUserIdFromTokenAsync(token);
            var userId = userGuidId?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Token refresh failed - could not extract user ID from token");
                return ServiceResult<object>.FailureResult("Invalid token");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Token refresh failed - user not found: {UserId}", userId);
                return ServiceResult<object>.FailureResult("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var newToken = await _jwtService.GenerateJwtTokenAsync(user, roles);
            var userDto = await UserMapper.ToDtoAsync(user, _userManager);

            _logger.LogInformation("Token refresh successful for user: {UserId} with roles: {Roles}", 
                userId, string.Join(", ", roles));

            var successResult = ServiceResult<object>.SuccessResult(new
            {
                Token = newToken.Token,
                TokenExpiry = newToken.Expiry,
                User = userDto,
                Roles = roles.ToList()
            });
            
            successResult.User = userDto;
            return successResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed with exception");
            return ServiceResult<object>.FailureResult("Token refresh failed");
        }
    }

    public async Task<ServiceResult<object>> ConfirmEmailAsync(string userId, string token)
    {
        try
        {
            _logger.LogInformation("Email confirmation attempt for user: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Email confirmation failed - user not found: {UserId}", userId);
                return ServiceResult<object>.FailureResult("User not found");
            }

            // Check if email is already confirmed
            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Email already confirmed for user: {UserId}", userId);
                return ServiceResult<object>.SuccessResult(new { }, "Email is already confirmed");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                _logger.LogInformation("Email confirmation successful for user: {UserId} ({Email})", userId, user.Email);

                // Update user's email confirmed status and last updated time
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Generate a fresh token for the user since email is now confirmed
                var roles = await _userManager.GetRolesAsync(user);
                var jwtToken = await _jwtService.GenerateJwtTokenAsync(user, roles);
                var userDto = await UserMapper.ToDtoAsync(user, _userManager);

                _logger.LogInformation("Generated JWT token for newly confirmed user: {UserId}", userId);

                var successResult = ServiceResult<object>.SuccessResult(new
                {
                    Token = jwtToken.Token,
                    TokenExpiry = jwtToken.Expiry,
                    User = userDto,
                    Roles = roles.ToList()
                }, "Email confirmed successfully. You can now access all features.");
                
                successResult.User = userDto;
                return successResult;
            }

            _logger.LogWarning("Email confirmation failed for user: {UserId}. Errors: {Errors}", 
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));

            return ServiceResult<object>.FailureResult(
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email confirmation failed with exception for user: {UserId}", userId);
            return ServiceResult<object>.FailureResult($"Email confirmation failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult<object>> ForgotPasswordAsync(string email)
    {
        try
        {
            _logger.LogInformation("Password reset request for email: {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Password reset request for non-existent email: {Email}", email);
                // Don't reveal that the user doesn't exist for security reasons
                return ServiceResult<object>.SuccessResult(new { }, 
                    "If the email exists, a password reset link has been sent");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogDebug("Generated password reset token for user: {UserId}", user.Id);

            // Send password reset email
            try
            {
                var emailSent = await _emailService.SendPasswordResetAsync(user.Email!, token, $"{user.FirstName} {user.LastName}");

                if (emailSent)
                {
                    _logger.LogInformation("Password reset email sent successfully to user: {UserId} ({Email})", user.Id, email);
                    return ServiceResult<object>.SuccessResult(new { }, 
                        "Password reset link has been sent to your email");
                }
                else
                {
                    _logger.LogWarning("Failed to send password reset email to user: {UserId} ({Email})", user.Id, email);
                    // Log the failure but don't reveal details to the user for security
                    return ServiceResult<object>.SuccessResult(new { }, 
                        "If the email exists, a password reset link has been sent");
                }
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Exception while sending password reset email to user: {UserId} ({Email})", user.Id, email);
                return ServiceResult<object>.SuccessResult(new { }, 
                    "If the email exists, a password reset link has been sent");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset request failed with exception for email: {Email}", email);
            return ServiceResult<object>.FailureResult("Password reset request failed");
        }
    }

    public async Task<ServiceResult<object>> ResetPasswordAsync(string email, string token, string newPassword)
    {
        try
        {
            _logger.LogInformation("Password reset attempt for email: {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Password reset failed - user not found for email: {Email}", email);
                return ServiceResult<object>.FailureResult("Invalid request");
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password reset successful for user: {UserId} ({Email})", user.Id, email);

                // Send password reset confirmation email
                try
                {
                    var emailSent = await _emailService.SendPasswordResetConfirmationAsync(user.Email!, $"{user.FirstName} {user.LastName}");

                    if (emailSent)
                    {
                        _logger.LogInformation("Password reset confirmation email sent to user: {UserId}", user.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send password reset confirmation email to user: {UserId}", user.Id);
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Exception while sending password reset confirmation email to user: {UserId}", user.Id);
                    // Log email sending result but don't fail the password reset if email fails
                }

                return ServiceResult<object>.SuccessResult(new { }, "Password reset successfully");
            }

            _logger.LogWarning("Password reset failed for user: {UserId} ({Email}). Errors: {Errors}", 
                user.Id, email, string.Join(", ", result.Errors.Select(e => e.Description)));

            return ServiceResult<object>.FailureResult(
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset failed with exception for email: {Email}", email);
            return ServiceResult<object>.FailureResult("Password reset failed");
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Retrieving user by ID: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return null;
            }

            var userDto = await UserMapper.ToDtoAsync(user, _userManager);
            _logger.LogDebug("Successfully retrieved user: {UserId} - {FirstName} {LastName}", userId, user.FirstName, user.LastName);

            return userDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> UpdateLastLoginAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Updating last login time for user: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Cannot update last login - user not found: {UserId}", userId);
                return false;
            }

            user.LastLoginAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                _logger.LogDebug("Successfully updated last login time for user: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Failed to update last login time for user: {UserId}. Errors: {Errors}", 
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login time for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<ServiceResult<object>> VerifyUserInformationAsync(VerifyUserInformationRequest request)
    {
        try
        {
            _logger.LogInformation("Document upload attempt for user: {UserId}, DocumentType: {DocumentType}", 
                request.UserId, request.UserPhysicalInformationType);

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("Document upload failed - user not found: {UserId}", request.UserId);
                return ServiceResult<object>.FailureResult("User not found");
            }

            // Validate file
            if (request.UserPhysicalInformation == null || request.UserPhysicalInformation.Length == 0)
            {
                _logger.LogWarning("Document upload failed - no file uploaded for user: {UserId}", request.UserId);
                return ServiceResult<object>.FailureResult("No file uploaded");
            }

            // Validate file type (allow common image and PDF formats)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".gif" };
            var fileExtension = Path.GetExtension(request.UserPhysicalInformation.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("Document upload failed - invalid file type {FileExtension} for user: {UserId}", 
                    fileExtension, request.UserId);
                return ServiceResult<object>.FailureResult(
                    "Invalid file type. Only JPG, PNG, PDF, and GIF files are allowed.");
            }

            // Validate file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (request.UserPhysicalInformation.Length > maxFileSize)
            {
                _logger.LogWarning("Document upload failed - file size {FileSize} exceeds limit for user: {UserId}", 
                    request.UserPhysicalInformation.Length, request.UserId);
                return ServiceResult<object>.FailureResult(
                    "File size exceeds the maximum limit of 5MB.");
            }

            _logger.LogDebug("File validation passed for user: {UserId}. File: {FileName}, Size: {FileSize}", 
                request.UserId, request.UserPhysicalInformation.FileName, request.UserPhysicalInformation.Length);

            // Create directory structure
            var uploadPath = Path.Combine("wwwroot", "uploads", "user-verification", user.Id.ToString());
            var fullUploadPath = Path.Combine(Directory.GetCurrentDirectory(), uploadPath);
            
            if (!Directory.Exists(fullUploadPath))
            {
                Directory.CreateDirectory(fullUploadPath);
                _logger.LogDebug("Created upload directory for user: {UserId}", request.UserId);
            }

            // Generate unique filename
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var documentType = request.UserPhysicalInformationType.ToString().ToLower();
            var fileName = $"{documentType}_{timestamp}{fileExtension}";
            var filePath = Path.Combine(fullUploadPath, fileName);
            var relativePath = Path.Combine("uploads", "user-verification", user.Id.ToString(), fileName).Replace("\\", "/");

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.UserPhysicalInformation.CopyToAsync(stream);
            }

            _logger.LogDebug("File saved successfully for user: {UserId} at path: {FilePath}", request.UserId, relativePath);

            // Update user record based on document type
            switch (request.UserPhysicalInformationType)
            {
                case UserPhysicalInformationType.UserIdentification:
                    user.UserIdentificationDocumentPath = relativePath;
                    break;
                case UserPhysicalInformationType.UserLocation:
                    user.UserLocationDocumentPath = relativePath;
                    break;
                default:
                    _logger.LogError("Invalid document type {DocumentType} for user: {UserId}", 
                        request.UserPhysicalInformationType, request.UserId);
                    return ServiceResult<object>.FailureResult("Invalid document type");
            }

            // Check if both documents are uploaded to set profile as complete
            var hasIdentification = !string.IsNullOrEmpty(user.UserIdentificationDocumentPath);
            var hasLocation = !string.IsNullOrEmpty(user.UserLocationDocumentPath);
            
            if (hasIdentification && hasLocation)
            {
                user.IsProfileComplete = true;
                _logger.LogInformation("Profile marked as complete for user: {UserId}", request.UserId);
            }

            user.UpdatedAt = DateTime.UtcNow;

            // Save changes
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user record after document upload for user: {UserId}. Errors: {Errors}", 
                    request.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));

                // Delete uploaded file if database update fails
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogDebug("Deleted uploaded file due to database update failure: {FilePath}", filePath);
                }

                return ServiceResult<object>.FailureResult("Failed to update user record");
            }

            var userDto = await UserMapper.ToDtoAsync(user, _userManager);

            _logger.LogInformation("Document upload successful for user: {UserId}. DocumentType: {DocumentType}, ProfileComplete: {IsProfileComplete}", 
                request.UserId, request.UserPhysicalInformationType, user.IsProfileComplete);

            var successResult = ServiceResult<object>.SuccessResult(new
            {
                User = userDto,
                DocumentType = request.UserPhysicalInformationType.ToString(),
                DocumentPath = relativePath,
                IsProfileComplete = user.IsProfileComplete
            }, $"{request.UserPhysicalInformationType} document uploaded successfully");
            
            successResult.User = userDto;
            return successResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document upload failed with exception for user: {UserId}", request.UserId);
            return ServiceResult<object>.FailureResult($"Document upload failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates default notification settings for a new user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Task</returns>
    private async Task CreateDefaultNotificationSettingsAsync(Guid userId)
    {
        try
        {
            _logger.LogDebug("Creating default notification settings for user: {UserId}", userId);

            var defaultSettings = new List<UserNotificationSetting>
            {
                // Booking Updates - enabled by default
                new UserNotificationSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NotificationType = SafeHabour.Models.Enums.NotificationType.BookingUpdates,
                    EmailNotificationEnabled = true,
                    InAppNotificationEnabled = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Messages - disabled by default
                new UserNotificationSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NotificationType = SafeHabour.Models.Enums.NotificationType.Messages,
                    EmailNotificationEnabled = false,
                    InAppNotificationEnabled = false,
                    CreatedAt = DateTime.UtcNow
                },
                // Payment Updates - enabled by default
                new UserNotificationSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NotificationType = SafeHabour.Models.Enums.NotificationType.PaymentUpdates,
                    EmailNotificationEnabled = true,
                    InAppNotificationEnabled = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Promotions & Marketing - disabled by default
                new UserNotificationSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NotificationType = SafeHabour.Models.Enums.NotificationType.PromotionsAndMarketing,
                    EmailNotificationEnabled = false,
                    InAppNotificationEnabled = false,
                    CreatedAt = DateTime.UtcNow
                },
                // Platform Updates - disabled by default
                new UserNotificationSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NotificationType = SafeHabour.Models.Enums.NotificationType.PlatformUpdates,
                    EmailNotificationEnabled = false,
                    InAppNotificationEnabled = false,
                    CreatedAt = DateTime.UtcNow
                },
                // System notifications - enabled by default (important for security)
                new UserNotificationSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NotificationType = SafeHabour.Models.Enums.NotificationType.System,
                    EmailNotificationEnabled = true,
                    InAppNotificationEnabled = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.UserNotificationSettings.AddRange(defaultSettings);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created {SettingCount} default notification settings for user: {UserId}", 
                defaultSettings.Count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create default notification settings for user: {UserId}", userId);
            throw; // Re-throw to ensure transaction rollback in calling methods
        }
    }

    /// <summary>
    /// Updates notification settings for a user
    /// </summary>
    /// <param name="request">The update notification setting request</param>
    /// <returns>Service result</returns>
    public async Task<ServiceResult<UserDto>> UpdateNotificationSettingAsync(UpdateNotificationSettingRequest request)
    {
        try
        {
            _logger.LogInformation("Updating notification setting for user: {UserId}, Type: {NotificationType}, Email: {EmailEnabled}, InApp: {InAppEnabled}", 
                request.UserId, request.NotificationType, request.EmailNotificationEnabled, request.InAppNotificationEnabled);

            if (!Guid.TryParse(request.UserId, out var userId))
            {
                _logger.LogWarning("Invalid user ID format in notification setting update: {UserId}", request.UserId);
                return ServiceResult<UserDto>.FailureResult("Invalid user ID format");
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found for notification setting update: {UserId}", request.UserId);
                return ServiceResult<UserDto>.FailureResult("User not found");
            }

            var setting = await _context.UserNotificationSettings
                .FirstOrDefaultAsync(s => s.UserId == userId && s.NotificationType == request.NotificationType);

            if (setting == null)
            {
                _logger.LogDebug("Creating new notification setting for user: {UserId}, Type: {NotificationType}", 
                    request.UserId, request.NotificationType);

                // Create new setting if it doesn't exist
                setting = new UserNotificationSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NotificationType = request.NotificationType,
                    EmailNotificationEnabled = request.EmailNotificationEnabled,
                    InAppNotificationEnabled = request.InAppNotificationEnabled,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserNotificationSettings.Add(setting);
            }
            else
            {
                _logger.LogDebug("Updating existing notification setting for user: {UserId}, Type: {NotificationType}", 
                    request.UserId, request.NotificationType);

                // Update existing setting
                setting.EmailNotificationEnabled = request.EmailNotificationEnabled;
                setting.InAppNotificationEnabled = request.InAppNotificationEnabled;
                setting.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Reload user with notification settings for response
            var userWithSettings = await _context.Users
                .Include(u => u.NotificationSettings)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var userDto = await UserMapper.ToDtoAsync(userWithSettings!, _userManager);

            _logger.LogInformation("Successfully updated notification setting for user: {UserId}, Type: {NotificationType}", 
                request.UserId, request.NotificationType);

            var successResult = ServiceResult<UserDto>.SuccessResult(userDto, "Notification settings updated successfully");
            successResult.User = userDto;
            return successResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update notification settings for user: {UserId}", request.UserId);
            return ServiceResult<UserDto>.FailureResult($"Failed to update notification settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets notification settings for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>List of UserNotificationSettingDto</returns>
    public async Task<List<UserNotificationSettingDto>> GetUserNotificationSettingsAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Retrieving notification settings for user: {UserId}", userId);

            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Invalid user ID format for notification settings retrieval: {UserId}", userId);
                return new List<UserNotificationSettingDto>();
            }

            var settings = await _context.UserNotificationSettings
                .Where(s => s.UserId == userGuid)
                .ToListAsync();

            _logger.LogInformation("Retrieved {SettingCount} notification settings for user: {UserId}", settings.Count, userId);

            return NotificationSettingMapper.ToDto(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification settings for user: {UserId}", userId);
            return new List<UserNotificationSettingDto>();
        }
    }

    /// <summary>
    /// Generates a random 6-digit two-factor authentication code
    /// </summary>
    /// <returns>6-digit code as string</returns>
    private string GenerateTwoFactorCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    /// <summary>
    /// Verifies a two-factor authentication code and completes the login process
    /// </summary>
    /// <param name="request">The two-factor verification request</param>
    /// <returns>Authentication result with token and user information</returns>
    public async Task<ServiceResult<object>> VerifyTwoFactorCodeAsync(VerifyTwoFactorCodeRequest request)
    {
        try
        {
            _logger.LogInformation("2FA code verification attempt for email: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("2FA verification failed - user not found for email: {Email}", request.Email);
                return ServiceResult<object>.FailureResult("Invalid request");
            }

            // Find valid, unused code
            var twoFactorCode = await _context.TwoFactorAuthCodes
                .Where(c => c.UserId == user.Id && 
                           c.Code == request.Code && 
                           !c.IsUsed && 
                           c.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (twoFactorCode == null)
            {
                _logger.LogWarning("2FA verification failed - invalid or expired code for user: {UserId} ({Email})", 
                    user.Id, request.Email);
                return ServiceResult<object>.FailureResult("Invalid or expired verification code");
            }

            _logger.LogDebug("Valid 2FA code found for user: {UserId}", user.Id);

            // Mark code as used
            twoFactorCode.IsUsed = true;
            twoFactorCode.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Update last login
            await UpdateLastLoginAsync(user.Id.ToString());

            // Generate token
            var roles = await _userManager.GetRolesAsync(user);
            var token = await _jwtService.GenerateJwtTokenAsync(user, roles);
            var userDto = await UserMapper.ToDtoAsync(user, _userManager);

            _logger.LogInformation("2FA verification successful for user: {UserId} ({Email})", user.Id, request.Email);

            var successResult = ServiceResult<object>.SuccessResult(new
            {
                Token = token.Token,
                TokenExpiry = token.Expiry,
                User = userDto,
                Roles = roles.ToList()
            }, "Login successful");
            
            successResult.User = userDto;
            return successResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "2FA verification failed with exception for email: {Email}", request.Email);
            return ServiceResult<object>.FailureResult($"Two-factor verification failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the two-factor authentication setting for a user
    /// </summary>
    /// <param name="request">The two-factor setting update request</param>
    /// <returns>Authentication result with success status</returns>
    public async Task<ServiceResult<UserDto>> UpdateTwoFactorSettingAsync(UpdateTwoFactorSettingRequest request)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out var userId))
            {
                return ServiceResult<UserDto>.FailureResult("Invalid user ID format");
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return ServiceResult<UserDto>.FailureResult("User not found");
            }

            // Update 2FA setting
            user.IsTwoFactorAuthenticationEnabled = request.EnableTwoFactor;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return ServiceResult<UserDto>.FailureResult(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // If disabling 2FA, invalidate any existing codes
            if (!request.EnableTwoFactor)
            {
                var existingCodes = await _context.TwoFactorAuthCodes
                    .Where(c => c.UserId == userId && !c.IsUsed)
                    .ToListAsync();

                foreach (var code in existingCodes)
                {
                    code.IsUsed = true;
                    code.UsedAt = DateTime.UtcNow;
                }

                if (existingCodes.Any())
                {
                    await _context.SaveChangesAsync();
                }
            }

            var userDto = await UserMapper.ToDtoAsync(user, _userManager);

            var successResult = ServiceResult<UserDto>.SuccessResult(userDto, 
                $"Two-factor authentication {(request.EnableTwoFactor ? "enabled" : "disabled")} successfully");
            successResult.User = userDto;
            return successResult;
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDto>.FailureResult($"Failed to update two-factor setting: {ex.Message}");
        }
    }

    /// <summary>
    /// Cleans up expired two-factor authentication codes
    /// This method should be called periodically (e.g., via a background service)
    /// </summary>
    /// <returns>Number of codes cleaned up</returns>
    public async Task<int> CleanupExpiredTwoFactorCodesAsync()
    {
        try
        {
            var expiredCodes = await _context.TwoFactorAuthCodes
                .Where(c => c.ExpiresAt <= DateTime.UtcNow && !c.IsUsed)
                .ToListAsync();

            if (expiredCodes.Any())
            {
                _context.TwoFactorAuthCodes.RemoveRange(expiredCodes);
                await _context.SaveChangesAsync();
            }

            return expiredCodes.Count;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    /// <summary>
    /// Resends email confirmation to a user
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Service result</returns>
    public async Task<ServiceResult<object>> ResendEmailConfirmationAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist for security reasons
                return ServiceResult<object>.SuccessResult(new { }, 
                    "If the email exists and is not confirmed, a confirmation email has been sent.");
            }

            if (user.EmailConfirmed)
            {
                return ServiceResult<object>.FailureResult("Email is already confirmed");
            }

            // Generate new email confirmation token and send verification email
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var emailSent = await _emailService.SendEmailConfirmationAsync(user.Email!, emailConfirmationToken, $"{user.FirstName} {user.LastName}", user.Id.ToString());

            if (emailSent)
            {
                return ServiceResult<object>.SuccessResult(new { }, 
                    "Confirmation email has been resent. Please check your email.");
            }
            else
            {
                return ServiceResult<object>.FailureResult("Failed to send confirmation email. Please try again later.");
            }
        }
        catch (Exception ex)
        {
            return ServiceResult<object>.FailureResult($"Failed to resend confirmation email: {ex.Message}");
        }
    }

}
