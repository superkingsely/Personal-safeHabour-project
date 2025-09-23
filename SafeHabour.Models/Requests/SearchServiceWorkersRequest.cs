using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Models.Requests;

public class SearchServiceWorkersRequest
{
    /// <summary>
    /// Page number for pagination (1-based)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Search term for service worker name, services, or bio
    /// </summary>
    [StringLength(100)]
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by specific service category
    /// </summary>
    [StringLength(50)]
    public string? ServiceCategory { get; set; }

    /// <summary>
    /// Filter by specific service name
    /// </summary>
    [StringLength(100)]
    public string? ServiceName { get; set; }

    /// <summary>
    /// Filter by language code (e.g., "en", "fr")
    /// </summary>
    [StringLength(20)]
    public string? LanguageCode { get; set; }

    /// <summary>
    /// Minimum hourly rate filter
    /// </summary>
    [Range(0.01, 10000.00)]
    public decimal? MinHourlyRate { get; set; }

    /// <summary>
    /// Maximum hourly rate filter
    /// </summary>
    [Range(0.01, 10000.00)]
    public decimal? MaxHourlyRate { get; set; }

    /// <summary>
    /// Search location latitude
    /// </summary>
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double? Latitude { get; set; }

    /// <summary>
    /// Search location longitude
    /// </summary>
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double? Longitude { get; set; }

    /// <summary>
    /// Maximum distance in kilometers (default: 50km)
    /// </summary>
    [Range(1, 500, ErrorMessage = "Radius must be between 1 and 500 kilometers")]
    public double RadiusKm { get; set; } = 50.0;

    /// <summary>
    /// Sort field options
    /// </summary>
    public ServiceWorkerSortBy SortBy { get; set; } = ServiceWorkerSortBy.Distance;

    /// <summary>
    /// Sort direction
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    /// <summary>
    /// Filter by minimum rating (1-5 stars)
    /// </summary>
    [Range(1, 5)]
    public int? MinRating { get; set; }

    /// <summary>
    /// Only show verified service workers
    /// </summary>
    public bool? VerifiedOnly { get; set; }

    /// <summary>
    /// Only show available service workers
    /// </summary>
    public bool? AvailableOnly { get; set; } = true;
}

public enum ServiceWorkerSortBy
{
    Distance = 0,
    HourlyRate = 1,
    Rating = 2,
    Name = 3,
    CreatedDate = 4
}

public enum SortDirection
{
    Ascending = 0,
    Descending = 1
}
