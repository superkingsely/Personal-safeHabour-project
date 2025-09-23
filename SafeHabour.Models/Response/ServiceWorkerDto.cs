using SafeHabour.Models.Requests;

namespace SafeHabour.Models.Response;

public class ServiceWorkerDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePicturePath { get; set; }
    
    // Address Information
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    
    // Location coordinates
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Service Worker specific fields
    public List<ServiceItem> Services { get; set; } = new();
    public List<LanguageItem> Languages { get; set; } = new();
    public decimal HourlyRate { get; set; }
}
