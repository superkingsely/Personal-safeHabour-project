// ScheduleResponse.cs
public class ScheduleResponse
{
    public Guid Id { get; set; }
    public Guid ServiceWorkerId { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
