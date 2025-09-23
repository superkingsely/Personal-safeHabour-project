using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SafeHabour.Models.Requests;

public class UpdateServiceWorkerRequest
{
    /// <summary>
    /// The user ID of the service worker to update
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// First name
    /// </summary>
    [StringLength(100)]
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name
    /// </summary>
    [StringLength(100)]
    public string? LastName { get; set; }

    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Gender
    /// </summary>
    [StringLength(10)]
    public string? Gender { get; set; }

    /// <summary>
    /// Bio/About information
    /// </summary>
    [StringLength(1000)]
    public string? Bio { get; set; }

    /// <summary>
    /// Profile picture file
    /// </summary>
    public IFormFile? ProfilePicture { get; set; }

    /// <summary>
    /// Street address
    /// </summary>
    [StringLength(500)]
    public string? StreetAddress { get; set; }

    /// <summary>
    /// City
    /// </summary>
    [StringLength(100)]
    public string? City { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    [StringLength(100)]
    public string? Country { get; set; }

    /// <summary>
    /// Postal code
    /// </summary>
    [StringLength(20)]
    public string? PostalCode { get; set; }

    /// <summary>
    /// User's latitude coordinate
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// User's longitude coordinate
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Services offered
    /// </summary>
    public List<ServiceItem>? Services { get; set; }

    /// <summary>
    /// Languages spoken
    /// </summary>
    public List<LanguageItem>? Languages { get; set; }

    /// <summary>
    /// Hourly rate
    /// </summary>
    [Range(0.01, 10000.00, ErrorMessage = "Hourly rate must be between 0.01 and 10,000.00")]
    public decimal? HourlyRate { get; set; }
}
