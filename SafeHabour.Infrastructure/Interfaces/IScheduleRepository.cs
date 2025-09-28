using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SafeHabour.Data.Entities;

namespace SafeHabour.Infrastructure.Interfaces
{
    public interface IScheduleRepository
    {
        Task<Schedule?> GetByIdAsync(Guid id);
        Task<List<Schedule>> GetByWorkerAsync(Guid workerId);
        Task<List<Schedule>> GetByWorkerAndDayAsync(Guid workerId, DayOfWeek dayOfWeek);
        Task AddAsync(Schedule schedule);
        Task UpdateAsync(Schedule schedule);
        Task DeleteAsync(Guid id);
    }
}
