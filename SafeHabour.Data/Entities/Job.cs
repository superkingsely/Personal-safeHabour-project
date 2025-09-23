using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Data.Entities;

public class Job
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Guid ClientId { get; set; }

    [Required]
    public ServiceType ServiceType { get; set; }

    [Required]
    public decimal Budget { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Open;

    [StringLength(200)]
    public string? Location { get; set; }

    public DateTime? PreferredStartDate { get; set; }

    public DateTime? PreferredEndDate { get; set; }

    public bool IsUrgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public virtual User Client { get; set; } = null!;
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public enum JobStatus
{
    Open = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    Disputed = 5
}

public enum ServiceType
{
    CareWorker = 1,
    SnowPlowing = 2,
    Cooking = 3,
    Cleaning = 4,
    Gardening = 5,
    Handyman = 6,
    PetCare = 7,
    Tutoring = 8,
    Other = 99
}
