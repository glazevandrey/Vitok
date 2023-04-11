using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.Models;
using web_server.Models.DBModels;

namespace web_server.Database.Repositories
{
    public class ScheduleRepository
    {
        DataContext _context;
        public ScheduleRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<int> AddSchedule(Schedule schedule)
        {
            await _context.Schedules.AddAsync(schedule);
            var res = await _context.SaveChangesAsync();
            return res;
        }
        public async Task<bool> RemoveSchedule(Schedule schedule)
        {
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateRange(List<Schedule> schedule)
        {
            _context.UpdateRange(schedule);
            await _context.SaveChangesAsync();
        }
        public async Task Update(Schedule schedule)
        {
            _context.Update(schedule);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> RemoveReschedule(RescheduledLessons reschedule)
        {
            _context.RescheduledLessons.Remove(reschedule);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<Schedule> GetScheduleById(int id)
        {
            return await _context.Schedules.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Schedule>> GetSchedulesByFunc(Func<Schedule, bool> func)
        {
            if(func == null)
            {
                return await _context.Schedules.ToListAsync();
            }

            return _context.Schedules.Where(func).ToList();
        }
        public async Task<Schedule> GetScheduleByFunc(Func<Schedule, bool> func)
        {
            return _context.Schedules.FirstOrDefault(func);
        }
        public async Task<List<RescheduledLessons>> GetReschedulesByFunc(Func<RescheduledLessons, bool> func)
        {
            if (func == null)
            {
                return await _context.RescheduledLessons.ToListAsync();
            }

            return _context.RescheduledLessons.Where(func).ToList();
        }
    }
}
