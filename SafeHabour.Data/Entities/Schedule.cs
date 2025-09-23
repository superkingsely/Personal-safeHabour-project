using System.ComponentModel.DataAnnotations;

namespace SafeHabour.Data.Entities;

public class Schedule
{
    public Guid Id { get; set; }

    [Required]
    public Guid ServiceWorkerId { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [StringLength(200)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual User ServiceWorker { get; set; } = null!;
}
