using Microsoft.EntityFrameworkCore;
using SafeHabour.Data.Data;
using SafeHabour.Data.Entities;
using SafeHabour.Infrastructure.Interfaces;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;

namespace SafeHabour.Infrastructure.Repositories;

public class SuperAdminRepository : ISuperAdminRepository
{
    private readonly ApiDbContext _context;

    public SuperAdminRepository(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<User>> GetAllUsersAsync(GetUsersRequest request)
    {
        var query = _context.Users
            .Include(u => u.ClientProfile)
            .Include(u => u.ServiceWorkerProfile)
            .Include(u => u.SuperAdminProfile)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower().Trim();
            query = query.Where(u => 
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                u.Email!.ToLower().Contains(searchTerm) ||
                u.UserName!.ToLower().Contains(searchTerm));
        }

        // Apply profile completion filter
        if (request.IsProfileComplete.HasValue)
        {
            query = query.Where(u => u.IsProfileComplete == request.IsProfileComplete.Value);
        }

        // Apply profile approval filter
        if (request.IsProfileApproved.HasValue)
        {
            query = query.Where(u => u.IsProfileApproved == request.IsProfileApproved.Value);
        }

        // Apply verification filter
        if (request.IsVerified.HasValue)
        {
            query = query.Where(u => u.IsVerified == request.IsVerified.Value);
        }

        // Apply active status filter
        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        // Apply user type filter
        if (!string.IsNullOrWhiteSpace(request.UserType))
        {
            switch (request.UserType.ToLower())
            {
                case "client":
                case "clientuser":
                    query = query.Where(u => u.ClientProfile != null);
                    break;
                case "serviceworker":
                case "service_worker":
                    query = query.Where(u => u.ServiceWorkerProfile != null);
                    break;
                case "superadmin":
                case "super_admin":
                    query = query.Where(u => u.SuperAdminProfile != null);
                    break;
            }
        }

        // Apply creation date range filter
        if (request.CreatedFrom.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= request.CreatedFrom.Value);
        }

        if (request.CreatedTo.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= request.CreatedTo.Value);
        }

        // Apply ordering (most recent first by default)
        query = query.OrderByDescending(u => u.CreatedAt);

        return new PagedList<User>(query, request.PageNumber, request.PageSize);
    }

    public async Task<ApprovalResult> ApproveUsersAsync(ApproveUsersRequest request)
    {
        var result = new ApprovalResult
        {
            Success = true,
            ProcessedCount = 0,
            FailedCount = 0
        };

        var validUserIds = new List<Guid>();

        // Validate user IDs
        foreach (var userIdString in request.UserIds)
        {
            if (Guid.TryParse(userIdString, out var userId))
            {
                validUserIds.Add(userId);
            }
            else
            {
                result.Errors.Add($"Invalid user ID format: {userIdString}");
                result.FailedCount++;
            }
        }

        if (!validUserIds.Any())
        {
            result.Success = false;
            result.Message = "No valid user IDs provided";
            return result;
        }

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Get users to update
            var users = await _context.Users
                .Where(u => validUserIds.Contains(u.Id))
                .ToListAsync();

            foreach (var user in users)
            {
                user.IsProfileApproved = request.IsApproved;
                user.UpdatedAt = DateTime.UtcNow;
                result.ProcessedCount++;
            }

            // Track users that weren't found
            var foundUserIds = users.Select(u => u.Id).ToList();
            var notFoundUserIds = validUserIds.Except(foundUserIds);
            foreach (var notFoundId in notFoundUserIds)
            {
                result.Errors.Add($"User not found: {notFoundId}");
                result.FailedCount++;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            result.Message = request.IsApproved 
                ? $"Successfully approved {result.ProcessedCount} user(s)"
                : $"Successfully rejected {result.ProcessedCount} user(s)";

            // TODO: Send notification emails to users about approval status
            // This would be implemented with the email service

        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = "An error occurred while processing user approvals";
            result.Errors.Add($"Database error: {ex.Message}");
        }

        return result;
    }

    public async Task<UserStatistics> GetUserStatisticsAsync()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var statistics = new UserStatistics
        {
            TotalUsers = await _context.Users.CountAsync(),
            ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
            InactiveUsers = await _context.Users.CountAsync(u => !u.IsActive),
            VerifiedUsers = await _context.Users.CountAsync(u => u.IsVerified),
            UnverifiedUsers = await _context.Users.CountAsync(u => !u.IsVerified),
            ProfileCompleteUsers = await _context.Users.CountAsync(u => u.IsProfileComplete),
            ProfileIncompleteUsers = await _context.Users.CountAsync(u => !u.IsProfileComplete),
            ApprovedUsers = await _context.Users.CountAsync(u => u.IsProfileApproved),
            PendingApprovalUsers = await _context.Users.CountAsync(u => !u.IsProfileApproved),
            ClientUsers = await _context.ClientUsers.CountAsync(),
            ServiceWorkerUsers = await _context.ServiceWorkerUsers.CountAsync(),
            SuperAdminUsers = await _context.SuperAdmins.CountAsync(),
            UsersRegisteredToday = await _context.Users.CountAsync(u => u.CreatedAt >= today),
            UsersRegisteredThisWeek = await _context.Users.CountAsync(u => u.CreatedAt >= weekStart),
            UsersRegisteredThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= monthStart)
        };

        return statistics;
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return null;
        }

        return await _context.Users
            .Include(u => u.ClientProfile)
            .Include(u => u.ServiceWorkerProfile)
            .Include(u => u.SuperAdminProfile)
            .Include(u => u.NotificationSettings)
            .Include(u => u.PostedJobs)
            .Include(u => u.Applications)
            .Include(u => u.ReviewsGiven)
            .Include(u => u.ReviewsReceived)
            .FirstOrDefaultAsync(u => u.Id == userGuid);
    }
}
