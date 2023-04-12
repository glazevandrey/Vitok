using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server.Database.Repositories
{
    public class ScheduleRepository
    {
        DataContext _context;
        IMapper _mapper;
        public ScheduleRepository(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<int> AddSchedule(Schedule schedule)
        {
            await _context.Schedules.AddAsync(_mapper.Map<ScheduleDTO>(schedule));
            var res = await _context.SaveChangesAsync();
            return res;
        }
        public async Task<bool> RemoveSchedule(Schedule schedule)
        {
            _context.Schedules.Remove(_mapper.Map<ScheduleDTO>(schedule));
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
            var mapped = await _context.Schedules.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == id);
            return _mapper.Map<Schedule>(mapped);
        }

        public async Task<List<Schedule>> GetSchedulesByFunc(Func<ScheduleDTO, bool> func)
        {
            if(func == null)
            {
                return _mapper.Map<List<Schedule>>(await _context.Schedules.Include(m => m.Course).ToListAsync());
            }

            return _mapper.Map<List<Schedule>>(_context.Schedules.Include(m => m.Course).Where(func).ToList());
        }
        public async Task<Schedule> GetScheduleByFunc(Func<ScheduleDTO, bool> func)
        {
            
                return (_mapper.Map<List<Schedule>>(_context.Schedules.Include(m=>m.Course).Where(func).ToList())).FirstOrDefault();
            
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
