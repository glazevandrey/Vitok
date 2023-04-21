using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            var mapped = _mapper.Map<ScheduleDTO>(schedule);
            await _context.Schedules.AddAsync(mapped);
            try
            {
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return mapped.Id;
        }
        public async Task<bool> RemoveSchedule(ScheduleDTO schedule)
        {
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateRange(List<ScheduleDTO> schedule)
        {
            try
            {

                await _context.SaveChangesAsync();
                foreach (var item in schedule)
                {
                    _context.Entry(item).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }
        public async Task Update(ScheduleDTO schedule)
        {
            try
            {
                var dd = _context.ChangeTracker.Entries();
                //var mapped = _mapper.Map<ScheduleDTO>(schedule);
                await _context.SaveChangesAsync();
                    _context.Entry(schedule).State = EntityState.Detached;

            }
            catch (Exception ex)
            {
                var ff = _context.ChangeTracker.Entries();
                Thread.Sleep(1000);
                await Update(schedule);
            }
     
        }
        public async Task Update(Schedule schedule)
        {
            try
            {
                var mapped = _mapper.Map<ScheduleDTO>(schedule);
                _context.Schedules.Update(mapped);
                await _context.SaveChangesAsync();
                _context.Entry(mapped).State = EntityState.Detached;

            }
            catch (Exception ex)
            {
                var ff = _context.ChangeTracker.Entries();
                Thread.Sleep(1000);
                await Update(schedule);
            }

        }
        public async Task<bool> RemoveReschedule(RescheduledLessons reschedule)
        {
            _context.RescheduledLessons.Remove(reschedule);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<ScheduleDTO> GetScheduleById(int id)
        {
            try
            {
                var mapped = await _context.Schedules.Include(m => m.Course).ThenInclude(m => m.Goal).FirstOrDefaultAsync(m => m.Id == id);
                //_context.Entry(mapped).State = EntityState.Detached;

                return mapped;
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
           // return _mapper.Map<ScheduleDTO>(mapped);
        }

        public async Task<List<ScheduleDTO>> GetSchedulesByFunc(Func<ScheduleDTO, bool> func)
        {
            try
            {
                if (func == null)
                {
                    var res = await _context.Schedules.Include(m => m.Course).ThenInclude(m => m.Goal).Include(m=>m.Tasks).Include(m=>m.RescheduledLessons).Include(m=>m.SkippedDates).Include(m=>m.ReadyDates).Include(m=>m.PaidLessons).ToListAsync();
                    return res;

                    //return _mapper.Map<List<Schedule>>(res);
                }

                return _context.Schedules.Include(m => m.Course).ThenInclude(m => m.Goal).Include(m => m.Tasks).Include(m => m.RescheduledLessons).Include(m => m.SkippedDates).Include(m => m.ReadyDates).Include(m => m.PaidLessons).Where(func).ToList();
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }
        public async Task<ScheduleDTO> GetScheduleByFunc(Func<ScheduleDTO, bool> func)
        {
            var res =  _context.Schedules.Include(m => m.Course).ThenInclude(m => m.Goal).Include(m=>m.SkippedDates).Include(m=>m.ReadyDates).Include(m=>m.PaidLessons).Include(m=>m.RescheduledLessons).Include(m=>m.Tasks).FirstOrDefault(func);
            //_context.Entry(res).State = EntityState.Detached;
            return res;
            //return (_mapper.Map<Schedule>(res));
            
        }
        public async Task<List<RescheduledLessons>> GetReschedulesByFunc(Func<RescheduledLessons, bool> func)
        {
            if (func == null)
            {
                var res2 = await _context.RescheduledLessons.ToListAsync();
                foreach (var item in res2)
                {


                    _context.Entry(item).State = EntityState.Detached;


                }

                return res2;
            }

            var res = _context.RescheduledLessons.Where(func).ToList();
            foreach (var item in res)
            {


                _context.Entry(item).State = EntityState.Detached;


            }
            return res;
        }
    }
}
