
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SafeHabour.Application.Interfaces;
using SafeHabour.Infrastructure.Interfaces;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;
using SafeHabour.Data.Entities;

namespace SafeHabour.Application.Managers
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _repo;
        public ScheduleService(IScheduleRepository repo) => _repo = repo;

        private static bool Overlaps(TimeOnly aStart, TimeOnly aEnd, TimeOnly bStart, TimeOnly bEnd)
            => aStart < bEnd && bStart < aEnd;

        public async Task<ServiceResult<ScheduleResponse>> CreateScheduleAsync(CreateScheduleRequest r)
        {
            if (!TimeOnly.TryParse(r.StartTime, out var start))
                return ServiceResult<ScheduleResponse>.FailureResult("Invalid StartTime format");
            if (!TimeOnly.TryParse(r.EndTime, out var end))
                return ServiceResult<ScheduleResponse>.FailureResult("Invalid EndTime format");
            if (end <= start)
                return ServiceResult<ScheduleResponse>.FailureResult("EndTime must be after StartTime");

            var existing = await _repo.GetByWorkerAndDayAsync(r.ServiceWorkerId, r.DayOfWeek);
            foreach (var e in existing)
            {
                if (Overlaps(start, end, e.StartTime, e.EndTime))
                    return ServiceResult<ScheduleResponse>.FailureResult("Time slot overlaps existing schedule.");
            }

            var entity = new Schedule
            {
                Id = Guid.NewGuid(),
                ServiceWorkerId = r.ServiceWorkerId,
                DayOfWeek = r.DayOfWeek,
                StartTime = start,
                EndTime = end,
                IsAvailable = r.IsAvailable,
                Notes = r.Notes
            };

            await _repo.AddAsync(entity);
            return ServiceResult<ScheduleResponse>.SuccessResult(Map(entity), "Schedule created successfully");
        }

        public async Task<ServiceResult<bool>> DeleteScheduleAsync(Guid id)
        {
            await _repo.DeleteAsync(id);
            return ServiceResult<bool>.SuccessResult(true, "Schedule deleted successfully");
        }

        public async Task<ServiceResult<IEnumerable<ScheduleResponse>>> GetWorkerSchedulesAsync(Guid workerId)
        {
            var list = await _repo.GetByWorkerAsync(workerId);
            var result = list.Select(Map).OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime);
            return ServiceResult<IEnumerable<ScheduleResponse>>.SuccessResult(result, "Schedules retrieved successfully");
        }

        public async Task<ServiceResult<IEnumerable<ScheduleResponse>>> GetWorkerSchedulesByDayAsync(Guid workerId, DayOfWeek dayOfWeek)
        {
            var list = await _repo.GetByWorkerAndDayAsync(workerId, dayOfWeek);
            var result = list.Select(Map).OrderBy(s => s.StartTime);
            return ServiceResult<IEnumerable<ScheduleResponse>>.SuccessResult(result, "Schedules for day retrieved successfully");
        }

        public async Task<ServiceResult<ScheduleResponse>> UpdateScheduleAsync(Guid id, UpdateScheduleRequest r)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult<ScheduleResponse>.FailureResult("Schedule not found");

            if (!TimeOnly.TryParse(r.StartTime, out var start))
                return ServiceResult<ScheduleResponse>.FailureResult("Invalid StartTime format");
            if (!TimeOnly.TryParse(r.EndTime, out var end))
                return ServiceResult<ScheduleResponse>.FailureResult("Invalid EndTime format");
            if (end <= start)
                return ServiceResult<ScheduleResponse>.FailureResult("EndTime must be after StartTime");

            var other = await _repo.GetByWorkerAndDayAsync(r.ServiceWorkerId, r.DayOfWeek);
            foreach (var o in other.Where(x => x.Id != id))
            {
                if (Overlaps(start, end, o.StartTime, o.EndTime))
                    return ServiceResult<ScheduleResponse>.FailureResult("Time slot overlaps existing schedule.");
            }

            existing.DayOfWeek = r.DayOfWeek;
            existing.StartTime = start;
            existing.EndTime = end;
            existing.IsAvailable = r.IsAvailable;
            existing.Notes = r.Notes;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return ServiceResult<ScheduleResponse>.SuccessResult(Map(existing), "Schedule updated successfully");
        }

        private static ScheduleResponse Map(Schedule e) => new()
        {
            Id = e.Id,
            ServiceWorkerId = e.ServiceWorkerId,
            DayOfWeek = e.DayOfWeek.ToString(),
            StartTime = e.StartTime.ToString("HH:mm"),
            EndTime = e.EndTime.ToString("HH:mm"),
            IsAvailable = e.IsAvailable,
            Notes = e.Notes
        };
    }
}
























// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using SafeHabour.Application.Interfaces;
// using SafeHabour.Infrastructure.Interfaces;
// using SafeHabour.Models.Requests;
// using SafeHabour.Models.Response;
// using SafeHabour.Data.Entities;

// namespace SafeHabour.Application.Managers
// {
//     public class ScheduleService : IScheduleService
//     {
//         private readonly IScheduleRepository _repo;
//         public ScheduleService(IScheduleRepository repo) => _repo = repo;

//         private static bool Overlaps(TimeOnly aStart, TimeOnly aEnd, TimeOnly bStart, TimeOnly bEnd)
//             => aStart < bEnd && bStart < aEnd;

//         public async Task<ScheduleResponse> CreateScheduleAsync(CreateScheduleRequest r)
//         {
//             if (!TimeOnly.TryParse(r.StartTime, out var start)) throw new ArgumentException("Invalid StartTime format");
//             if (!TimeOnly.TryParse(r.EndTime, out var end)) throw new ArgumentException("Invalid EndTime format");
//             if (end <= start) throw new ArgumentException("EndTime must be after StartTime");

//             // check overlap on same worker & day
//             var existing = await _repo.GetByWorkerAndDayAsync(r.ServiceWorkerId, r.DayOfWeek);
//             foreach (var e in existing)
//             {
//                 if (Overlaps(start, end, e.StartTime, e.EndTime))
//                     throw new InvalidOperationException("Time slot overlaps existing schedule.");
//             }

//             var entity = new Schedule
//             {
//                 Id = Guid.NewGuid(),
//                 ServiceWorkerId = r.ServiceWorkerId,
//                 DayOfWeek = r.DayOfWeek,
//                 StartTime = start,
//                 EndTime = end,
//                 IsAvailable = r.IsAvailable,
//                 Notes = r.Notes
//             };
//             await _repo.AddAsync(entity);
//             return Map(entity);
//         }

//         public async Task DeleteScheduleAsync(Guid id)
//         {
//             await _repo.DeleteAsync(id);
//         }

        
//         public async Task<IEnumerable<ScheduleResponse>> GetWorkerSchedulesForDayAsync(Guid workerId, DayOfWeek dayOfWeek)
//         {
//             var list = await _repo.GetByWorkerAndDayAsync(workerId, dayOfWeek);
//             return list.Select(Map).OrderBy(s => s.StartTime);
//         }

//         public async Task<IEnumerable<ScheduleResponse>> GetWorkerSchedulesAsync(Guid workerId)
//         {
//             var list = await _repo.GetByWorkerAsync(workerId);
//             return list.Select(Map).OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime);
//         }

//         public async Task<ScheduleResponse> UpdateScheduleAsync(Guid id, CreateScheduleRequest r)
//         {
//             var existing = await _repo.GetByIdAsync(id);
//             if (existing == null) throw new KeyNotFoundException("Schedule not found");

//             if (!TimeOnly.TryParse(r.StartTime, out var start)) throw new ArgumentException("Invalid StartTime format");
//             if (!TimeOnly.TryParse(r.EndTime, out var end)) throw new ArgumentException("Invalid EndTime format");
//             if (end <= start) throw new ArgumentException("EndTime must be after StartTime");

//             // check overlap with other items
//             var other = await _repo.GetByWorkerAndDayAsync(r.ServiceWorkerId, r.DayOfWeek);
//             foreach (var o in other.Where(x => x.Id != id))
//             {
//                 if (Overlaps(start, end, o.StartTime, o.EndTime))
//                     throw new InvalidOperationException("Time slot overlaps existing schedule.");
//             }

//             existing.DayOfWeek = r.DayOfWeek;
//             existing.StartTime = start;
//             existing.EndTime = end;
//             existing.IsAvailable = r.IsAvailable;
//             existing.Notes = r.Notes;
//             existing.UpdatedAt = DateTime.UtcNow;

//             await _repo.UpdateAsync(existing);
//             return Map(existing);
//         }

//        private static ScheduleResponse Map(Schedule e) => new()
//         {
//             Id = e.Id,
//             ServiceWorkerId = e.ServiceWorkerId,
//             DayOfWeek = e.DayOfWeek.ToString(),   // ✅ Convert enum → string
//             StartTime = e.StartTime.ToString("HH:mm"),
//             EndTime = e.EndTime.ToString("HH:mm"),
//             IsAvailable = e.IsAvailable,
//             Notes = e.Notes
//         };

//     }
// }
