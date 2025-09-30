using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SafeHabour.Models.Requests;
using SafeHabour.Models.Response;

namespace SafeHabour.Application.Interfaces
{
    public interface IScheduleService
    {
        Task<ServiceResult<ScheduleResponse>> CreateScheduleAsync(CreateScheduleRequest request);
        Task<ServiceResult<IEnumerable<ScheduleResponse>>> GetWorkerSchedulesAsync(Guid workerId);
        Task<ServiceResult<IEnumerable<ScheduleResponse>>> GetWorkerSchedulesByDayAsync(Guid workerId, DayOfWeek dayOfWeek);
        Task<ServiceResult<ScheduleResponse>> UpdateScheduleAsync(Guid id, UpdateScheduleRequest request);
        Task<ServiceResult<bool>> DeleteScheduleAsync(Guid id);
    }
}




    // public interface IScheduleService
    // {
    //     Task<IEnumerable<ScheduleResponse>> GetWorkerSchedulesAsync(Guid workerId);
    //     Task<IEnumerable<ScheduleResponse>> GetWorkerSchedulesForDayAsync(Guid workerId, DayOfWeek dayOfWeek);
    //     Task<ScheduleResponse> CreateScheduleAsync(CreateScheduleRequest request);
    //     Task<ScheduleResponse> UpdateScheduleAsync(Guid id, CreateScheduleRequest request);
    //     Task DeleteScheduleAsync(Guid id);
    // }