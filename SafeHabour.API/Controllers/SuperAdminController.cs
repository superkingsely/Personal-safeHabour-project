using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeHabour.Application.Interfaces;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;
using System.Security.Claims;

namespace SafeHabour.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class SuperAdminController : ControllerBase
{
    private readonly ISuperAdminService _superAdminService;
    private readonly ILogger<SuperAdminController> _logger;

    public SuperAdminController(
        ISuperAdminService superAdminService,
        ILogger<SuperAdminController> logger)
    {
        _superAdminService = superAdminService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of all users with search and filtering capabilities
    /// </summary>
    /// <param name="request">Search and filter parameters</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet("users")]
    [Authorize(Roles = "SuperAdmin")] // Only SuperAdmins can access this
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ServiceResult<PagedList<UserDto>>
                {
                    Success = false,
                    Message = "Invalid request parameters",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var users = await _superAdminService.GetAllUsersAsync(request);

            return Ok(new ServiceResult<PagedList<UserDto>>
            {
                Success = true,
                Data = users,
                Message = $"Retrieved {users.List.Count} users successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving users");
            return StatusCode(500, new ServiceResult<PagedList<UserDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving users"
            });
        }
    }

    /// <summary>
    /// Gets detailed information about a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("users/{userId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        try
        {
            var user = await _superAdminService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new
            {
                Success = true,
                Data = user,
                Message = "User retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user {UserId}", userId);
            return StatusCode(500, new
            {
                Success = false,
                Message = "An error occurred while retrieving user"
            });
        }
    }

    /// <summary>
    /// Approves or rejects multiple users
    /// </summary>
    /// <param name="request">List of user IDs and approval status</param>
    /// <returns>Result of the approval operation</returns>
    [HttpPut("users/approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ApproveUsers([FromBody] ApproveUsersRequest request)
    {
        try
        {
            var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var result = await _superAdminService.ApproveUsersAsync(request);

            _logger.LogInformation("SuperAdmin {AdminId} {Action} {Count} users", 
                adminUserId, 
                request.IsApproved ? "approved" : "rejected", 
                result.ProcessedCount);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = result.Message,
                    Errors = result.Errors,
                    ProcessedCount = result.ProcessedCount,
                    FailedCount = result.FailedCount
                });
            }

            return Ok(new
            {
                Success = true,
                Message = result.Message,
                ProcessedCount = result.ProcessedCount,
                FailedCount = result.FailedCount,
                Errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while approving users");
            return StatusCode(500, new
            {
                Success = false,
                Message = "An error occurred while processing user approvals"
            });
        }
    }

    /// <summary>
    /// Gets system-wide user statistics
    /// </summary>
    /// <returns>User statistics</returns>
    [HttpGet("statistics")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetUserStatistics()
    {
        try
        {
            var statistics = await _superAdminService.GetUserStatisticsAsync();

            return Ok(new
            {
                Success = true,
                Data = statistics,
                Message = "Statistics retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user statistics");
            return StatusCode(500, new
            {
                Success = false,
                Message = "An error occurred while retrieving statistics"
            });
        }
    }

    /// <summary>
    /// Bulk approve all pending users (users with IsProfileApproved = false)
    /// </summary>
    /// <returns>Result of the bulk approval operation</returns>
    [HttpPut("users/bulk-approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> BulkApproveUsers([FromBody] BulkApprovalRequest request)
    {
        try
        {
            var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            // Get all pending users based on filters
            var getUsersRequest = new GetUsersRequest
            {
                IsProfileApproved = false, // Only get non-approved users
                IsActive = request.ApproveOnlyActiveUsers ? true : null,
                IsProfileComplete = request.ApproveOnlyCompleteProfiles ? true : null,
                PageNumber = 1,
                PageSize = 1000 // Large page size for bulk operation
            };

            var pendingUsers = await _superAdminService.GetAllUsersAsync(getUsersRequest);

            if (!pendingUsers.List.Any())
            {
                return Ok(new
                {
                    Success = true,
                    Message = "No pending users found for approval",
                    ProcessedCount = 0
                });
            }

            var userIds = pendingUsers.List.Select(u => u.Id.ToString()).ToList();
            var approveRequest = new ApproveUsersRequest
            {
                UserIds = userIds,
                IsApproved = true,
                Reason = request.Reason ?? "Bulk approval by SuperAdmin"
            };

            var result = await _superAdminService.ApproveUsersAsync(approveRequest);

            _logger.LogInformation("SuperAdmin {AdminId} bulk approved {Count} users", 
                adminUserId, 
                result.ProcessedCount);

            return Ok(new
            {
                Success = result.Success,
                Message = result.Message,
                ProcessedCount = result.ProcessedCount,
                FailedCount = result.FailedCount,
                Errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during bulk user approval");
            return StatusCode(500, new
            {
                Success = false,
                Message = "An error occurred during bulk approval"
            });
        }
    }
}

/// <summary>
/// Request model for bulk approval operations
/// </summary>
public class BulkApprovalRequest
{
    /// <summary>
    /// Only approve users with active status
    /// </summary>
    public bool ApproveOnlyActiveUsers { get; set; } = true;

    /// <summary>
    /// Only approve users with complete profiles
    /// </summary>
    public bool ApproveOnlyCompleteProfiles { get; set; } = true;

    /// <summary>
    /// Reason for bulk approval
    /// </summary>
    public string? Reason { get; set; }
}
