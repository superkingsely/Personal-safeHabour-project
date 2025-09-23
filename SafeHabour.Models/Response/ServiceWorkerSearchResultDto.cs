namespace SafeHabour.Models.Response;

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public double? SearchLatitude { get; set; }
    public double? SearchLongitude { get; set; }
    public double? RadiusKm { get; set; }
    public bool UsedFallbackLocation { get; set; }
    public string? FallbackLocationSource { get; set; }
}

public class ServiceWorkerSearchResultDto : ServiceWorkerDto
{
    /// <summary>
    /// Distance from search location in kilometers
    /// </summary>
    public double? DistanceKm { get; set; }

    /// <summary>
    /// Average rating from reviews (1-5 stars)
    /// </summary>
    public double? AverageRating { get; set; }

    /// <summary>
    /// Total number of reviews
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// Whether the service worker is currently available
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Whether the service worker is verified
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// When the service worker joined the platform
    /// </summary>
    public DateTime JoinedDate { get; set; }

    /// <summary>
    /// Last active date
    /// </summary>
    public DateTime? LastActiveDate { get; set; }
}
