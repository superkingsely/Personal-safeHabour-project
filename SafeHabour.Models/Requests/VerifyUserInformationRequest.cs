using Microsoft.AspNetCore.Http;
using SafeHabour.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Models.Requests;

public class VerifyUserInformationRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public IFormFile UserPhysicalInformation { get; set; } = null!;

    [Required]
    public UserPhysicalInformationType UserPhysicalInformationType { get; set; }
}
