using SafeHabour.Data.Entities;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;

namespace SafeHabour.Data.DTOMapper
{
    public static class ScheduleMapper
    {
        public static ScheduleResponse ToResponse(Schedule entity)
        {
            return new ScheduleResponse
            {
                Id = entity.Id,
                ServiceWorkerId = entity.ServiceWorkerId,
                DayOfWeek = entity.DayOfWeek.ToString(),
                StartTime = entity.StartTime.ToString("HH:mm"), // TimeOnly -> string
                EndTime = entity.EndTime.ToString("HH:mm"),     // TimeOnly -> string
                IsAvailable = entity.IsAvailable,
                Notes = entity.Notes,
                CreatedAt = entity.CreatedAt
            };
        }

        public static Schedule ToEntity(CreateScheduleRequest dto)
        {
            return new Schedule
            {
                Id = Guid.NewGuid(),
                ServiceWorkerId = dto.ServiceWorkerId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = TimeOnly.Parse(dto.StartTime), // string -> TimeOnly
                EndTime = TimeOnly.Parse(dto.EndTime),     // string -> TimeOnly
                IsAvailable = dto.IsAvailable,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateEntityFromDto(Schedule entity, CreateScheduleRequest dto)
        {
            entity.DayOfWeek = dto.DayOfWeek;
            entity.StartTime = TimeOnly.Parse(dto.StartTime); // string -> TimeOnly
            entity.EndTime = TimeOnly.Parse(dto.EndTime);     // string -> TimeOnly
            entity.IsAvailable = dto.IsAvailable;
            entity.Notes = dto.Notes;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
