using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SafeHabour.Data.Entities;

public class User : IdentityUser<Guid>
{

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }

    [StringLength(500)]
    public string? ProfilePicturePath { get; set; }

    // Address Information
    [StringLength(500)]
    public string? StreetAddress { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    // Location coordinates
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool IsProfileComplete { get; set; }

    public bool IsVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsProfileApproved { get; set; } = false;

    // Two-Factor Authentication
    public bool IsTwoFactorAuthenticationEnabled { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    // Stripe-related properties
    [StringLength(255)]
    public string? StripeCustomerId { get; set; }

    [StringLength(255)]
    public string? StripeConnectAccountId { get; set; }

    public bool IsStripeConnectEnabled { get; set; }

    // User verification document paths
    [StringLength(500)]
    public string? UserIdentificationDocumentPath { get; set; }

    [StringLength(500)]
    public string? UserLocationDocumentPath { get; set; }

    // Navigation properties
    public virtual ICollection<Job> PostedJobs { get; set; } = new List<Job>();
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public virtual ICollection<Payment> ClientPayments { get; set; } = new List<Payment>();
    public virtual ICollection<Payment> ServiceWorkerPayments { get; set; } = new List<Payment>();
    public virtual ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
    public virtual ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
    public virtual ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
    public virtual ICollection<Dispute> InitiatedDisputes { get; set; } = new List<Dispute>();
    public virtual ICollection<Dispute> RespondentDisputes { get; set; } = new List<Dispute>();
    public virtual ICollection<Dispute> AssignedDisputes { get; set; } = new List<Dispute>();
    public virtual ICollection<UserNotificationSetting> NotificationSettings { get; set; } = new List<UserNotificationSetting>();
    public virtual ICollection<TwoFactorAuthCode> TwoFactorAuthCodes { get; set; } = new List<TwoFactorAuthCode>();
    public virtual ICollection<PushNotification> PushNotifications { get; set; } = new List<PushNotification>();
    public virtual ClientUser? ClientProfile { get; set; }
    public virtual ServiceWorkerUser? ServiceWorkerProfile { get; set; }
    public virtual SuperAdmin? SuperAdminProfile { get; set; }
}
