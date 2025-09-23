using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Models.Requests;

public class ConfirmEmailRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}
