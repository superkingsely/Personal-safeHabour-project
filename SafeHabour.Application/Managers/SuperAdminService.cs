using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafeHabour.Application.Interfaces;
using SafeHabour.Data.Data;
using SafeHabour.Data.DTOMapper.User;
using SafeHabour.Data.Entities;
using SafeHabour.Infrastructure.Interfaces;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;

namespace SafeHabour.Application.Managers;

public class SuperAdminService : ISuperAdminService
{
    private readonly ISuperAdminRepository _superAdminRepository;
    private readonly UserManager<User> _userManager;
    private readonly ApiDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<SuperAdminService> _logger;

    public SuperAdminService(
        ISuperAdminRepository superAdminRepository,
        UserManager<User> userManager,
        ApiDbContext context,
        IEmailService emailService,
        ILogger<SuperAdminService> logger)
    {
        _superAdminRepository = superAdminRepository;
        _userManager = userManager;
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<PagedList<UserDto>> GetAllUsersAsync(GetUsersRequest request)
    {
        try
        {
            _logger.LogInformation("Retrieving users with filters - Page: {PageNumber}, Size: {PageSize}, Search: {SearchTerm}, IsActive: {IsActive}, IsApproved: {IsApproved}", 
                request.PageNumber, request.PageSize, request.SearchTerm, request.IsActive, request.IsProfileApproved);

            var users = await _superAdminRepository.GetAllUsersAsync(request);
            
            _logger.LogDebug("Retrieved {UserCount} users from repository", users.List.Count);

            // Convert User entities to UserDto
            var userDtos = new List<UserDto>();
            foreach (var user in users.List)
            {
                var userDto = await UserMapper.ToDtoAsync(user, _userManager);
                userDtos.Add(userDto);
            }

            _logger.LogInformation("Successfully retrieved and mapped {UserCount} users out of {TotalUsers} total", 
                userDtos.Count, users.TotalItems);

            // Create a custom PagedList result with the same pagination metadata
            return new PagedList<UserDto>(userDtos, users.TotalItems, users.PageNumber, users.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving users with request: {@Request}", request);
            throw;
        }
    }

    public async Task<ApprovalResult> ApproveUsersAsync(ApproveUsersRequest request)
    {
        try
        {
            _logger.LogInformation("Processing approval request for {UserCount} users. IsApproved: {IsApproved}, Reason: {Reason}", 
                request.UserIds.Count, request.IsApproved, request.Reason);

            // Validate users before approval
            var validationErrors = new List<string>();
            var validUserIds = new List<string>();

            foreach (var userId in request.UserIds)
            {
                var validation = await ValidateUserForApprovalAsync(userId);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("User {UserId} failed validation: {Issues}", userId, string.Join(", ", validation.Issues));
                    validationErrors.AddRange(validation.Issues.Select(issue => $"User {userId}: {issue}"));
                }
                else if (!validation.CanBeApproved && request.IsApproved)
                {
                    _logger.LogWarning("User {UserId} has approval warnings: {Warnings}", userId, string.Join(", ", validation.Warnings));
                    validationErrors.AddRange(validation.Warnings.Select(warning => $"User {userId}: {warning}"));
                }
                else
                {
                    validUserIds.Add(userId);
                }
            }

            _logger.LogInformation("Validation completed. Valid users: {ValidCount}, Invalid users: {InvalidCount}", 
                validUserIds.Count, validationErrors.Count);

            // If approving users, only proceed with valid ones
            if (request.IsApproved && validationErrors.Any())
            {
                _logger.LogWarning("Approval request rejected due to validation errors: {ErrorCount} errors found", validationErrors.Count);
                return new ApprovalResult
                {
                    Success = false,
                    Message = "Some users failed validation for approval",
                    Errors = validationErrors
                };
            }

            // Proceed with approval/rejection
            var approvalRequest = new ApproveUsersRequest
            {
                UserIds = validUserIds,
                IsApproved = request.IsApproved,
                Reason = request.Reason
            };

            _logger.LogInformation("Proceeding with approval/rejection for {ValidUserCount} users", validUserIds.Count);
            var result = await _superAdminRepository.ApproveUsersAsync(approvalRequest);

            // Send notification emails to users about their approval status
            if (result.Success && result.ProcessedCount > 0)
            {
                _logger.LogInformation("Sending approval notifications to {ProcessedCount} users", result.ProcessedCount);
                await SendApprovalNotificationsAsync(validUserIds, request.IsApproved, request.Reason);
            }

            // Add validation errors to the result if any
            if (validationErrors.Any())
            {
                result.Errors.AddRange(validationErrors);
            }

            _logger.LogInformation("Approval process completed. Success: {Success}, Processed: {ProcessedCount}, Failed: {FailedCount}", 
                result.Success, result.ProcessedCount, result.FailedCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing approval request for {UserCount} users", request.UserIds.Count);
            throw;
        }
    }

    public async Task<UserStatistics> GetUserStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving user statistics");
            
            var statistics = await _superAdminRepository.GetUserStatisticsAsync();
            
            _logger.LogInformation("Retrieved user statistics - Total: {TotalUsers}, Active: {ActiveUsers}, Pending: {PendingUsers}, Clients: {ClientUsers}, ServiceWorkers: {ServiceWorkerUsers}", 
                statistics.TotalUsers, statistics.ActiveUsers, statistics.PendingApprovalUsers, statistics.ClientUsers, statistics.ServiceWorkerUsers);
            
            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user statistics");
            throw;
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Retrieving user by ID: {UserId}", userId);
            
            var user = await _superAdminRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return null;
            }

            var userDto = await UserMapper.ToDtoAsync(user, _userManager);
            
            _logger.LogInformation("Successfully retrieved user: {UserId} - {FirstName} {LastName}", 
                userId, user.FirstName, user.LastName);
            
            return userDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user by ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<ApprovalResult> BulkApproveUsersAsync(bool approveOnlyActiveUsers = true, bool approveOnlyCompleteProfiles = true, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Starting bulk approval process. ActiveOnly: {ActiveOnly}, CompleteProfilesOnly: {CompleteProfilesOnly}, Reason: {Reason}", 
                approveOnlyActiveUsers, approveOnlyCompleteProfiles, reason);

            // Get all pending users based on criteria
            var getUsersRequest = new GetUsersRequest
            {
                IsProfileApproved = false, // Only get non-approved users
                IsActive = approveOnlyActiveUsers ? true : null,
                IsProfileComplete = approveOnlyCompleteProfiles ? true : null,
                PageNumber = 1,
                PageSize = 1000 // Large page size for bulk operation
            };

            var pendingUsers = await _superAdminRepository.GetAllUsersAsync(getUsersRequest);
            
            _logger.LogInformation("Found {PendingUserCount} pending users for bulk approval", pendingUsers.List.Count);

            if (!pendingUsers.List.Any())
            {
                _logger.LogInformation("No pending users found for bulk approval");
                return new ApprovalResult
                {
                    Success = true,
                    Message = "No pending users found for approval",
                    ProcessedCount = 0
                };
            }

            var userIds = pendingUsers.List.Select(u => u.Id.ToString()).ToList();
            var approveRequest = new ApproveUsersRequest
            {
                UserIds = userIds,
                IsApproved = true,
                Reason = reason ?? "Bulk approval by SuperAdmin"
            };

            _logger.LogInformation("Proceeding with bulk approval of {UserIdCount} users", userIds.Count);
            var result = await ApproveUsersAsync(approveRequest);
            
            _logger.LogInformation("Bulk approval completed. Success: {Success}, Processed: {ProcessedCount}", 
                result.Success, result.ProcessedCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during bulk approval process");
            throw;
        }
    }

    public async Task<PendingApprovalSummary> GetPendingApprovalSummaryAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving pending approval summary");

            var pendingUsersQuery = _context.Users
                .Include(u => u.ClientProfile)
                .Include(u => u.ServiceWorkerProfile)
                .Where(u => !u.IsProfileApproved);

            var summary = new PendingApprovalSummary
            {
                TotalPendingUsers = await pendingUsersQuery.CountAsync(),
                PendingActiveUsers = await pendingUsersQuery.CountAsync(u => u.IsActive),
                PendingCompleteProfileUsers = await pendingUsersQuery.CountAsync(u => u.IsProfileComplete),
                PendingVerifiedUsers = await pendingUsersQuery.CountAsync(u => u.IsVerified),
                PendingClientUsers = await pendingUsersQuery.CountAsync(u => u.ClientProfile != null),
                PendingServiceWorkerUsers = await pendingUsersQuery.CountAsync(u => u.ServiceWorkerProfile != null),
                PendingUsersWithDocuments = await pendingUsersQuery.CountAsync(u => 
                    !string.IsNullOrEmpty(u.UserIdentificationDocumentPath) || 
                    !string.IsNullOrEmpty(u.UserLocationDocumentPath))
            };

            _logger.LogInformation("Pending approval summary - Total: {TotalPending}, Active: {ActivePending}, Complete: {CompletePending}, Clients: {ClientsPending}, ServiceWorkers: {ServiceWorkersPending}", 
                summary.TotalPendingUsers, summary.PendingActiveUsers, summary.PendingCompleteProfileUsers, summary.PendingClientUsers, summary.PendingServiceWorkerUsers);

            // Get 5 most recent pending users
            var recentPendingUsers = await pendingUsersQuery
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            _logger.LogDebug("Retrieved {RecentUserCount} recent pending users for summary", recentPendingUsers.Count);

            foreach (var user in recentPendingUsers)
            {
                var userDto = await UserMapper.ToDtoAsync(user, _userManager);
                summary.RecentPendingUsers.Add(userDto);
            }

            _logger.LogInformation("Successfully generated pending approval summary");
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving pending approval summary");
            throw;
        }
    }

    public async Task<UserValidationResult> ValidateUserForApprovalAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Validating user for approval: {UserId}", userId);

            var result = new UserValidationResult
            {
                IsValid = true,
                CanBeApproved = true
            };

            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Invalid user ID format provided: {UserId}", userId);
                result.IsValid = false;
                result.CanBeApproved = false;
                result.Issues.Add("Invalid user ID format");
                result.Message = "User ID is not in a valid format";
                return result;
            }

            var user = await _context.Users
                .Include(u => u.ClientProfile)
                .Include(u => u.ServiceWorkerProfile)
                .FirstOrDefaultAsync(u => u.Id == userGuid);

            if (user == null)
            {
                _logger.LogWarning("User not found for validation: {UserId}", userId);
                result.IsValid = false;
                result.CanBeApproved = false;
                result.Issues.Add("User not found");
                result.Message = "User does not exist in the system";
                return result;
            }

            _logger.LogDebug("Validating user {UserId} - {FirstName} {LastName}", userId, user.FirstName, user.LastName);

            // Check if user is already approved
            if (user.IsProfileApproved)
            {
                _logger.LogInformation("User {UserId} is already approved", userId);
                result.CanBeApproved = false;
                result.Warnings.Add("User is already approved");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("User {UserId} account is inactive", userId);
                result.Warnings.Add("User account is inactive");
            }

            // Check if user has completed their profile
            if (!user.IsProfileComplete)
            {
                _logger.LogWarning("User {UserId} profile is not complete", userId);
                result.Warnings.Add("User profile is not complete");
            }

            // Check if user has uploaded required documents
            var hasIdentificationDoc = !string.IsNullOrEmpty(user.UserIdentificationDocumentPath);
            var hasLocationDoc = !string.IsNullOrEmpty(user.UserLocationDocumentPath);

            if (!hasIdentificationDoc)
            {
                _logger.LogWarning("User {UserId} has not uploaded identification document", userId);
                result.Warnings.Add("User has not uploaded identification document");
            }

            if (!hasLocationDoc)
            {
                _logger.LogWarning("User {UserId} has not uploaded location document", userId);
                result.Warnings.Add("User has not uploaded location document");
            }

            // Check if user has a profile type (Client or ServiceWorker)
            var hasProfile = user.ClientProfile != null || user.ServiceWorkerProfile != null;
            if (!hasProfile)
            {
                _logger.LogError("User {UserId} does not have a client or service worker profile", userId);
                result.Issues.Add("User does not have a client or service worker profile");
                result.CanBeApproved = false;
            }

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("User {UserId} email is not confirmed", userId);
                result.Warnings.Add("User email is not confirmed");
            }

            // Set final message
            if (result.Issues.Any())
            {
                result.Message = "User has critical issues that prevent approval";
            }
            else if (result.Warnings.Any())
            {
                result.Message = "User has warnings but can still be approved";
            }
            else
            {
                result.Message = "User is ready for approval";
            }

            _logger.LogInformation("User validation completed for {UserId}. Can be approved: {CanBeApproved}, Issues: {IssueCount}, Warnings: {WarningCount}", 
                userId, result.CanBeApproved, result.Issues.Count, result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user for approval: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<ApprovalAuditEntry>> GetUserApprovalAuditTrailAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Retrieving approval audit trail for user: {UserId}", userId);

            // This would typically come from an audit table
            // For now, we'll return a simplified version based on user update history
            var auditEntries = new List<ApprovalAuditEntry>();

            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Invalid user ID format for audit trail: {UserId}", userId);
                return auditEntries;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userGuid);
            if (user == null)
            {
                _logger.LogWarning("User not found for audit trail: {UserId}", userId);
                return auditEntries;
            }

            _logger.LogDebug("Found user for audit trail: {UserId} - {FirstName} {LastName}", userId, user.FirstName, user.LastName);

            // In a real application, you would have an audit table tracking all approval changes
            // For now, we'll create a simple entry based on current status
            if (user.IsProfileApproved && user.UpdatedAt.HasValue)
            {
                auditEntries.Add(new ApprovalAuditEntry
                {
                    Id = Guid.NewGuid(),
                    UserId = userGuid,
                    WasApproved = true,
                    ActionDate = user.UpdatedAt.Value,
                    Reason = "Profile approved by SuperAdmin",
                    ApproverName = "SuperAdmin" // In real app, you'd track the actual admin who approved
                });
            }

            _logger.LogInformation("Retrieved {EntryCount} audit trail entries for user: {UserId}", auditEntries.Count, userId);
            return auditEntries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving approval audit trail for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Sends notification emails to users about their approval status
    /// </summary>
    /// <param name="userIds">List of user IDs</param>
    /// <param name="isApproved">Whether users were approved or rejected</param>
    /// <param name="reason">Reason for the action</param>
    private async Task SendApprovalNotificationsAsync(List<string> userIds, bool isApproved, string? reason)
    {
        try
        {
            _logger.LogInformation("Sending approval notifications to {UserCount} users. IsApproved: {IsApproved}", userIds.Count, isApproved);

            var successCount = 0;
            var failureCount = 0;

            foreach (var userIdString in userIds)
            {
                if (Guid.TryParse(userIdString, out var userId))
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        try
                        {
                            var subject = isApproved ? "Profile Approved - Welcome to SafeHabour!" : "Profile Review Required";
                            var message = isApproved 
                                ? $"Hello {user.FirstName}, your profile has been approved and you can now access all SafeHabour features."
                                : $"Hello {user.FirstName}, your profile requires additional review. Reason: {reason ?? "Please contact support for more information."}";

                            await _emailService.SendApprovalNotificationAsync(user.Email, subject, message, $"{user.FirstName} {user.LastName}");
                            
                            _logger.LogDebug("Successfully sent approval notification to user {UserId} ({Email})", userId, user.Email);
                            successCount++;
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning(emailEx, "Failed to send approval notification to user {UserId} ({Email})", userId, user.Email);
                            failureCount++;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("User {UserId} not found or has no email address for notification", userId);
                        failureCount++;
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid user ID format for notification: {UserIdString}", userIdString);
                    failureCount++;
                }
            }

            _logger.LogInformation("Approval notification process completed. Success: {SuccessCount}, Failed: {FailureCount}", successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending approval notifications to {UserCount} users", userIds.Count);
            // Don't throw - we don't want notification failures to fail the approval process
        }
    }
}
