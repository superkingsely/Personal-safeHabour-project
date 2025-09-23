using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Models.Requests;

public class UpdateTwoFactorSettingRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public bool EnableTwoFactor { get; set; }
}
