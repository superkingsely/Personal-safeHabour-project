using SafeHabour.Data.Entities;
using SafeHabour.Infrastructure.Interfaces;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;

namespace SafeHabour.Application.Interfaces;

public interface ISuperAdminService
{
    /// <summary>
    /// Gets a paginated list of all users in the system with search and filtering capabilities
    /// </summary>
    /// <param name="request">Search and filter parameters</param>
    /// <returns>Paginated list of user DTOs</returns>
    Task<PagedList<UserDto>> GetAllUsersAsync(GetUsersRequest request);

    /// <summary>
    /// Approves or rejects multiple users
    /// </summary>
    /// <param name="request">List of user IDs and approval status</param>
    /// <returns>Result containing the number of users processed and any errors</returns>
    Task<ApprovalResult> ApproveUsersAsync(ApproveUsersRequest request);

    /// <summary>
    /// Gets detailed statistics about users in the system
    /// </summary>
    /// <returns>User statistics including counts by status</returns>
    Task<UserStatistics> GetUserStatisticsAsync();

    /// <summary>
    /// Gets a specific user by ID with all related data
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User DTO with related data or null if not found</returns>
    Task<UserDto?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Bulk approves all pending users based on specified criteria
    /// </summary>
    /// <param name="approveOnlyActiveUsers">Only approve active users</param>
    /// <param name="approveOnlyCompleteProfiles">Only approve users with complete profiles</param>
    /// <param name="reason">Reason for bulk approval</param>
    /// <returns>Result of the bulk approval operation</returns>
    Task<ApprovalResult> BulkApproveUsersAsync(bool approveOnlyActiveUsers = true, bool approveOnlyCompleteProfiles = true, string? reason = null);

    /// <summary>
    /// Gets a summary of users pending approval with counts by criteria
    /// </summary>
    /// <returns>Pending approval summary</returns>
    Task<PendingApprovalSummary> GetPendingApprovalSummaryAsync();

    /// <summary>
    /// Validates if a user can be approved based on business rules
    /// </summary>
    /// <param name="userId">User ID to validate</param>
    /// <returns>Validation result with any issues found</returns>
    Task<UserValidationResult> ValidateUserForApprovalAsync(string userId);

    /// <summary>
    /// Gets audit trail for user approval actions
    /// </summary>
    /// <param name="userId">User ID to get audit trail for</param>
    /// <returns>List of approval audit entries</returns>
    Task<List<ApprovalAuditEntry>> GetUserApprovalAuditTrailAsync(string userId);
}

/// <summary>
/// Summary of users pending approval
/// </summary>
public class PendingApprovalSummary
{
    public int TotalPendingUsers { get; set; }
    public int PendingActiveUsers { get; set; }
    public int PendingCompleteProfileUsers { get; set; }
    public int PendingVerifiedUsers { get; set; }
    public int PendingClientUsers { get; set; }
    public int PendingServiceWorkerUsers { get; set; }
    public int PendingUsersWithDocuments { get; set; }
    public List<UserDto> RecentPendingUsers { get; set; } = new List<UserDto>();
}

/// <summary>
/// Result of user validation for approval
/// </summary>
public class UserValidationResult
{
    public bool IsValid { get; set; }
    public bool CanBeApproved { get; set; }
    public List<string> Issues { get; set; } = new List<string>();
    public List<string> Warnings { get; set; } = new List<string>();
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Audit entry for approval actions
/// </summary>
public class ApprovalAuditEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public bool WasApproved { get; set; }
    public string? Reason { get; set; }
    public DateTime ActionDate { get; set; }
    public string ApproverName { get; set; } = string.Empty;
}
