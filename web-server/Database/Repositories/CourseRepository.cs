using AutoMapper;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server.Database.Repositories
{
    public class CourseRepository
    {
        DataContext _context;
        IMapper _mapper;
        public CourseRepository(IMapper mapper,DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }
        public async Task<bool> RemoveCourse(int id)
        {
            var rem = _context.Courses.FirstOrDefault(m=>m.Id == id);
            _context.Courses.Remove(rem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Course> GetCourseById(int id)
        {
            var res = await _context.Courses.Include(m=>m.Goal).FirstOrDefaultAsync(m => m.Id == id);

                _context.Entry(res).State = EntityState.Detached;
            
            var dsd = _context.ChangeTracker.Entries();
            foreach (var item in dsd)
            {
                item.State = EntityState.Detached;
            }
            return _mapper.Map<Course>(res);
        }

        public async Task<List<Course>> GetAllCourses()
        {

            var res = await _context.Courses.Include(m=>m.Goal).ToListAsync();

            foreach (var item in res)
            {
                _context.Entry(item).State = EntityState.Detached;
            }
            var dsd = _context.ChangeTracker.Entries();
            foreach (var item in dsd)
            {
                item.State = EntityState.Detached;
            }
            var dd = _mapper.Map<List<Course>>(res);
            return dd;
        }
        public async Task<List<Goal>> GetAllGoals()
        {
            var res = await _context.Goals.Include(m=>m.Courses).ToListAsync();
         
            foreach (var item in res)
            {
                _context.Entry(item).State = EntityState.Detached;
                
            }
            var dd = _context.ChangeTracker.Entries();
            foreach (var item in dd)
            {
                item.State = EntityState.Detached;
            }
            return  _mapper.Map<List<Goal>>(res);
        }
        public async Task<List<Tariff>> GetAllTariffs()
        {
            return await _context.Tariffs.ToListAsync();
        }
        public async Task<Goal> GetGoalById(int id)
        {
            var res = await _context.Goals.Include(m=>m.Courses).FirstOrDefaultAsync(m => m.Id == id);
            return _mapper.Map<Goal>(res);
        }
        public async Task<bool> Update(CourseDTO course)
        {
             _context.Courses.Update(course);
            await _context.SaveChangesAsync();
            _context.Entry(course).State = EntityState.Detached;
            var dd = _context.ChangeTracker.Entries();
            foreach (var item in dd)
            {
                item.State = EntityState.Detached;
            }
            return true;
        }
        public async Task<bool> AddCourse(CourseDTO course)
        {
            _context.Courses.Add(course);
            try
            {
                await _context.SaveChangesAsync();


            }
            catch (System.Exception ex)
            {

                throw ex;
            }
            
            return true;
        }
    }
}
