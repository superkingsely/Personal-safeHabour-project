using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Data.Entities;

public class Review
{
    public Guid Id { get; set; }

    [Required]
    public Guid JobId { get; set; }

    [Required]
    public Guid ReviewerId { get; set; }

    [Required]
    public Guid RevieweeId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }

    public bool IsPublic { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Job Job { get; set; } = null!;
    public virtual User Reviewer { get; set; } = null!;
    public virtual User Reviewee { get; set; } = null!;
}
