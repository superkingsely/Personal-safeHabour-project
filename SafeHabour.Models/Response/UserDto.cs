namespace SafeHabour.Models.Response;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
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
    
    public bool IsProfileComplete { get; set; }
    public bool IsVerified { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public bool IsStripeConnectEnabled { get; set; }
    public bool HasIdentificationDocument { get; set; }
    public bool HasLocationDocument { get; set; }
    public bool IsTwoFactorAuthenticationEnabled { get; set; }
    // public int UserType { get; set; }
    public List<UserNotificationSettingDto> NotificationSettings { get; set; } = new List<UserNotificationSettingDto>();
}
