using SafeHabour.Data.Entities;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;

namespace SafeHabour.Data.DTOMapper.Schedule;

public static class ScheduleMapper
{
    public static ScheduleResponse ToResponse(Schedule entity)
    {
        return new ScheduleResponse
        {
            Id = entity.Id,
            ServiceWorkerId = entity.ServiceWorkerId,
            DayOfWeek = entity.DayOfWeek.ToString(),
            StartTime = entity.StartTime.ToString("HH:mm"),
            EndTime = entity.EndTime.ToString("HH:mm"),
            IsAvailable = entity.IsAvailable,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt
        };
    }

    public static Schedule ToEntity(ScheduleRequest request)
    {
        return new Schedule
        {
            Id = Guid.NewGuid(),
            ServiceWorkerId = request.ServiceWorkerId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsAvailable = true,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateEntityFromRequest(Schedule entity, ScheduleRequest request)
    {
        entity.DayOfWeek = request.DayOfWeek;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.Notes = request.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
