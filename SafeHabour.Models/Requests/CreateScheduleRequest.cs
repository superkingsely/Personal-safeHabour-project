// CreateScheduleRequest.cs
public class CreateScheduleRequest
{
    public Guid ServiceWorkerId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty; // client sends string
    public string EndTime { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public string? Notes { get; set; }
}
