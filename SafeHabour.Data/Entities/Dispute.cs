using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Data.Entities;

public class Dispute
{
    public Guid Id { get; set; }

    [Required]
    public Guid JobId { get; set; }

    [Required]
    public Guid InitiatorId { get; set; }

    [Required]
    public Guid RespondentId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public DisputeStatus Status { get; set; } = DisputeStatus.Open;

    public DisputeType Type { get; set; }

    public Guid? AssignedAdminId { get; set; }

    public DateTime? ResolvedAt { get; set; }

    [StringLength(1000)]
    public string? Resolution { get; set; }

    public decimal? RefundAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Job Job { get; set; } = null!;
    public virtual User Initiator { get; set; } = null!;
    public virtual User Respondent { get; set; } = null!;
    public virtual User? AssignedAdmin { get; set; }
}

public enum DisputeStatus
{
    Open = 1,
    InReview = 2,
    Resolved = 3,
    Closed = 4
}

public enum DisputeType
{
    PaymentIssue = 1,
    ServiceQuality = 2,
    NoShow = 3,
    Cancellation = 4,
    Other = 5
}
