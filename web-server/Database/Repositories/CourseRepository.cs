using AutoMapper;
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
            var res = await _context.Courses.FirstOrDefaultAsync(m => m.Id == id);
            _context.Entry(res).State = EntityState.Detached;
            return _mapper.Map<Course>(res);
        }

        public async Task<List<Course>> GetAllCourses()
        {
            var res = await _context.Courses.ToListAsync();

            foreach (var item in res)
            {
                
                
                
            }
                
            return _mapper.Map<List<Course>>(res);
        }
        public async Task<List<Goal>> GetAllGoals()
        {
            var res = await _context.Goals.ToListAsync();
            foreach (var item in res)
            {
                
                
                
            }

            return res;
        }
        public async Task<List<Tariff>> GetAllTariffs()
        {
            return await _context.Tariffs.ToListAsync();
        }
        public async Task<Goal> GetGoalById(int id)
        {
            var res = await _context.Goals.FirstOrDefaultAsync(m => m.Id == id);
            _context.Entry(res).State = EntityState.Detached;
            return res;
        }
        public async Task<bool> AddCourse(CourseDTO course)
        {
            _context.Courses.Add(course);

            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}
