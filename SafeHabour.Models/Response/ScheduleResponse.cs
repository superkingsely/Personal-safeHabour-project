using System;

namespace SafeHabour.Models.Response
{
    public class ScheduleResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceWorkerId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string StartTime { get; set; } = default!; // "08:00"
        public string EndTime { get; set; } = default!;
        public bool IsAvailable { get; set; }
        public string? Notes { get; set; }
    }
}
