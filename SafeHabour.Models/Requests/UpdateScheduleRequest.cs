using System;

namespace SafeHabour.Models.Requests
{
    public class UpdateScheduleRequest
    {
        public Guid ServiceWorkerId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string StartTime { get; set; } = string.Empty; // e.g. "09:00"
        public string EndTime { get; set; } = string.Empty;   // e.g. "17:00"
        public bool IsAvailable { get; set; }
        public string? Notes { get; set; }
    }
}
