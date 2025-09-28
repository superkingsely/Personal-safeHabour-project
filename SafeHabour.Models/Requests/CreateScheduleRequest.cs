using System;

namespace SafeHabour.Models.Requests
{
    public class CreateScheduleRequest
    {
        public Guid ServiceWorkerId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }

        // Use string "HH:mm" for JSON compatibility
        public string StartTime { get; set; } = default!; 
        public string EndTime { get; set; } = default!;

        public bool IsAvailable { get; set; } = true;
        public string? Notes { get; set; }
    }
}
