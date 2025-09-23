using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Models.Requests;

public class CreateServiceWorkerUserRequest
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
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

    [StringLength(500)]
    public string? Bio { get; set; }

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public List<ServiceItem> Services { get; set; } = new();

    [Required]
    public List<LanguageItem> Languages { get; set; } = new();
    
    [Required]
    [Range(0.01, 10000.00, ErrorMessage = "Hourly rate must be between 0.01 and 10,000.00")]
    public decimal HourlyRate { get; set; }

    /// <summary>
    /// User's latitude coordinate
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// User's longitude coordinate
    /// </summary>
    public double? Longitude { get; set; }
}

public class ServiceItem
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
}

public class LanguageItem
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty; // e.g., "en", "fr", "es"
    
    [Required]
    [StringLength(20)]
    public string ProficiencyLevel { get; set; } = string.Empty; // e.g., "Native", "Fluent", "Intermediate", "Basic"
    
    public bool IsNative { get; set; } = false;
}
