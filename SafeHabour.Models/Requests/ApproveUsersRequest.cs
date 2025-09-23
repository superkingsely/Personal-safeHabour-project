using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Models.Requests;

public class ApproveUsersRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one user ID must be provided")]
    public List<string> UserIds { get; set; } = new List<string>();

    /// <summary>
    /// Approval status to set for the users
    /// </summary>
    [Required]
    public bool IsApproved { get; set; }

    /// <summary>
    /// Optional reason for approval/rejection
    /// </summary>
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; set; }
}
