using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Data.Entities;

public class Application
{
    public Guid Id { get; set; }

    [Required]
    public Guid JobId { get; set; }

    [Required]
    public Guid ServiceWorkerId { get; set; }

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public decimal ProposedRate { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime? RejectedAt { get; set; }

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    // Navigation properties
    public virtual Job Job { get; set; } = null!;
    public virtual User ServiceWorker { get; set; } = null!;
}

public enum ApplicationStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3,
    Withdrawn = 4
}
