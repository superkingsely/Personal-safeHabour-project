using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Data.Entities;

public class Payment
{
    public Guid Id { get; set; }

    [Required]
    public Guid JobId { get; set; }

    [Required]
    public Guid ClientId { get; set; }

    [Required]
    public Guid ServiceWorkerId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [StringLength(255)]
    public string? StripePaymentIntentId { get; set; }

    [StringLength(255)]
    public string? StripeChargeId { get; set; }

    public DateTime? AuthorizedAt { get; set; }

    public DateTime? CapturedAt { get; set; }

    public DateTime? RefundedAt { get; set; }

    public decimal? RefundAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Job Job { get; set; } = null!;
    public virtual User Client { get; set; } = null!;
    public virtual User ServiceWorker { get; set; } = null!;
}

public enum PaymentStatus
{
    Pending = 1,
    Authorized = 2,
    Captured = 3,
    Failed = 4,
    Refunded = 5,
    PartiallyRefunded = 6
}
