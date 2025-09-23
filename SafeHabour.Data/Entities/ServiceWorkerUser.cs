using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using SafeHabour.Models.Requests;

namespace SafeHabour.Data.Entities;

public class ServiceWorkerUser : BaseModel
{
    public string? Bio { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }
    
    /// <summary>
    /// JSON serialized list of ServiceItem objects
    /// </summary>
    public string ServicesJson { get; set; } = "[]";
    
    /// <summary>
    /// JSON serialized list of LanguageItem objects
    /// </summary>
    public string LanguagesJson { get; set; } = "[]";
    
    public decimal HourlyRate { get; set; }
    
    /// <summary>
    /// User's latitude coordinate
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// User's longitude coordinate
    /// </summary>
    public double? Longitude { get; set; }
    
    /// <summary>
    /// Property to handle Services as List<ServiceItem> with JSON serialization
    /// </summary>
    [NotMapped]
    public List<ServiceItem> Services
    {
        get => string.IsNullOrEmpty(ServicesJson) ? new List<ServiceItem>() 
               : JsonSerializer.Deserialize<List<ServiceItem>>(ServicesJson) ?? new List<ServiceItem>();
        set => ServicesJson = JsonSerializer.Serialize(value);
    }
    
    /// <summary>
    /// Property to handle Languages as List<LanguageItem> with JSON serialization
    /// </summary>
    [NotMapped]
    public List<LanguageItem> Languages
    {
        get => string.IsNullOrEmpty(LanguagesJson) ? new List<LanguageItem>() 
               : JsonSerializer.Deserialize<List<LanguageItem>>(LanguagesJson) ?? new List<LanguageItem>();
        set => LanguagesJson = JsonSerializer.Serialize(value);
    }
    

    public DateTime DateOfBirth { get; set; }
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
}
