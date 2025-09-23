using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Models.Requests;

/// <summary>
/// Request DTO for resending email confirmation
/// </summary>
public class ResendEmailConfirmationRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;
}
