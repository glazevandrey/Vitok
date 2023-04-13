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
            int id = 0;
            try
            {
                id= await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return id;
        }
        public async Task<bool> RemoveSchedule(Schedule schedule)
        {
            _context.Schedules.Remove(_mapper.Map<ScheduleDTO>(schedule));
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateRange(List<Schedule> schedule)
        {
            try
            {

                foreach (var item in _mapper.Map<List<ScheduleDTO>>(schedule))
                {
                    
                }
                _context.UpdateRange(_mapper.Map<List<ScheduleDTO>>(schedule));

                await _context.SaveChangesAsync();
                foreach (var item in _mapper.Map<List<ScheduleDTO>>(await _context.Schedules.Include(m => m.Tasks).ToListAsync()))
                {
                    
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }
        public async Task Update(Schedule schedule)
        {
            _context.Update(_mapper.Map<ScheduleDTO>(schedule));
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
            _context.Entry(mapped).State = EntityState.Detached;

            return _mapper.Map<Schedule>(mapped);
        }

        public async Task<List<Schedule>> GetSchedulesByFunc(Func<ScheduleDTO, bool> func)
        {
            if(func == null)
            {
                var res = await _context.Schedules.Include(m => m.Course).AsNoTracking().ToListAsync();
                foreach (var item in res)
                {
                    
                }
                return _mapper.Map<List<Schedule>>(res);
            }

            return _mapper.Map<List<Schedule>>(_context.Schedules.AsNoTracking().Include(m => m.Course).Where(func).ToList());
        }
        public async Task<Schedule> GetScheduleByFunc(Func<ScheduleDTO, bool> func)
        {

            var res =  _context.Schedules.AsNoTracking().Include(m => m.Course).FirstOrDefault(func);
            _context.Entry(res).State = EntityState.Detached;
         
            return (_mapper.Map<Schedule>(res));
            
        }
        public async Task<List<RescheduledLessons>> GetReschedulesByFunc(Func<RescheduledLessons, bool> func)
        {
            if (func == null)
            {
                var res2 = await _context.RescheduledLessons.AsNoTracking().ToListAsync();
                foreach (var item in res2)
                {
                    
                    
                    

                }

                return res2;
            }

            var res = _context.RescheduledLessons.Where(func).ToList();
            return res;
        }
    }
}
