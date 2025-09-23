using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Data.Entities;

public class Transfer
{
    public Guid Id { get; set; }

    [Required]
    public Guid PaymentId { get; set; }

    [Required]
    public Guid ServiceWorkerId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public TransferStatus Status { get; set; } = TransferStatus.Pending;

    [StringLength(255)]
    public string? StripeTransferId { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? FailedAt { get; set; }

    [StringLength(500)]
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Payment Payment { get; set; } = null!;
    public virtual User ServiceWorker { get; set; } = null!;
}

public enum TransferStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}
