using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Models.Requests;

public class CreateClientUserRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Phone]
    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    public int ClientType { get; set; }

    /// <summary>
    /// User's latitude coordinate
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// User's longitude coordinate
    /// </summary>
    public double? Longitude { get; set; }
}
