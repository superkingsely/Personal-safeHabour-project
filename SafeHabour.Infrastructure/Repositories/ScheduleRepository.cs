using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SafeHabour.Data;
using SafeHabour.Data.Entities;
using SafeHabour.Infrastructure.Interfaces;

namespace SafeHabour.Infrastructure.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly SafeHabourDbContext _db;
        public ScheduleRepository(SafeHabourDbContext db) => _db = db;

        public async Task AddAsync(Schedule schedule)
        {
            _db.Schedules.Add(schedule);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var e = await _db.Schedules.FindAsync(id);
            if (e != null) { _db.Schedules.Remove(e); await _db.SaveChangesAsync(); }
        }

        public async Task<Schedule?> GetByIdAsync(Guid id) =>
            await _db.Schedules.FirstOrDefaultAsync(s => s.Id == id);

        public async Task<List<Schedule>> GetByWorkerAsync(Guid workerId) =>
            await _db.Schedules.Where(s => s.ServiceWorkerId == workerId).ToListAsync();

        public async Task<List<Schedule>> GetByWorkerAndDayAsync(Guid workerId, DayOfWeek dayOfWeek) =>
            await _db.Schedules.Where(s => s.ServiceWorkerId == workerId && s.DayOfWeek == dayOfWeek).ToListAsync();

        public async Task UpdateAsync(Schedule schedule)
        {
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
        }
    }
}
