using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Models.Requests;

public class GetUsersRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Search term for user's first name, last name, or email
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by profile completion status
    /// </summary>
    public bool? IsProfileComplete { get; set; }

    /// <summary>
    /// Filter by profile approval status
    /// </summary>
    public bool? IsProfileApproved { get; set; }

    /// <summary>
    /// Filter by user verification status
    /// </summary>
    public bool? IsVerified { get; set; }

    /// <summary>
    /// Filter by user active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter by user type (ClientUser, ServiceWorker)
    /// </summary>
    public string? UserType { get; set; }

    /// <summary>
    /// Filter by creation date range - start date
    /// </summary>
    public DateTime? CreatedFrom { get; set; }

    /// <summary>
    /// Filter by creation date range - end date
    /// </summary>
    public DateTime? CreatedTo { get; set; }
}
