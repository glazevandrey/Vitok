using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.Models.DBModels;
using web_server.Models.DBModels.DTO;

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
            return _mapper.Map<Course>(await _context.Courses.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id));
        }

        public async Task<List<Course>> GetAllCourses()
        {
            return _mapper.Map<List<Course>>( await _context.Courses.ToListAsync());
        }
        public async Task<List<Goal>> GetAllGoals()
        {
            return await _context.Goals.ToListAsync();
        }
        public async Task<List<Tariff>> GetAllTariffs()
        {
            return await _context.Tariffs.ToListAsync();
        }
        public async Task<Goal> GetGoalById(int id)
        {
            return await _context.Goals.FirstOrDefaultAsync(m => m.Id == id);
        }
        public async Task<bool> AddCourse(CourseDTO course)
        {
            _context.Courses.Add(course);

            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}
