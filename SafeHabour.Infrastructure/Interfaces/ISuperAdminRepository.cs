using SafeHabour.Data.Entities;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;

namespace SafeHabour.Infrastructure.Interfaces;

public interface ISuperAdminRepository
{
    /// <summary>
    /// Gets a paginated list of all users in the system with search and filtering capabilities
    /// </summary>
    /// <param name="request">Search and filter parameters</param>
    /// <returns>Paginated list of users</returns>
    Task<PagedList<User>> GetAllUsersAsync(GetUsersRequest request);

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
    /// <returns>User with related data or null if not found</returns>
    Task<User?> GetUserByIdAsync(string userId);
}

/// <summary>
/// Result of the user approval operation
/// </summary>
public class ApprovalResult
{
    public bool Success { get; set; }
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// System-wide user statistics
/// </summary>
public class UserStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int VerifiedUsers { get; set; }
    public int UnverifiedUsers { get; set; }
    public int ProfileCompleteUsers { get; set; }
    public int ProfileIncompleteUsers { get; set; }
    public int ApprovedUsers { get; set; }
    public int PendingApprovalUsers { get; set; }
    public int ClientUsers { get; set; }
    public int ServiceWorkerUsers { get; set; }
    public int SuperAdminUsers { get; set; }
    public int UsersRegisteredToday { get; set; }
    public int UsersRegisteredThisWeek { get; set; }
    public int UsersRegisteredThisMonth { get; set; }
}
