using AutoMapper;
using AutoMapper.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server.Database.Repositories
{
    public class UserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(IMapper mapper, DataContext data)
        {
            _mapper = mapper;
            _context = data;
            
        }

        public async Task<bool> AddRegistration(Registration registration)
        {
            var f = _mapper.Map<RegistrationDTO>(registration);
            _context.Registrations.Add(f);
            await _context.SaveChangesAsync();
            _context.Entry(f).State = EntityState.Detached;
            return true;
        }
        public async Task<Registration> GetRegistrationByUserId(int userId)
        {
            var reg = await _context.Registrations.FirstOrDefaultAsync(m=>m.UserId == userId);

            return _mapper.Map<Registration>(reg);
        }
        public async Task<List<Tutor>> GetAllTutors()
        {
            try
            {
                var model = await _context.Tutors.Include(m => m.Schedules).Include(m => m.UserDates).Include(m => m.BalanceHistory).Include(m => m.Courses).ThenInclude(c=>c.Course).ThenInclude(c=>c.Goal).ToListAsync();
                return _mapper.Map<List<Tutor>>(model);
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }
      
        public async Task<List<User>> GetAll()
        {

            var tutors = await _context.Tutors.Include(m => m.Schedules).Include(m => m.UserDates).Include(m => m.BalanceHistory).ThenInclude(m => m.CashFlow).Include(m => m.Courses).ThenInclude(c => c.Course).ThenInclude(c => c.Goal).ToListAsync();
            var students = await _context.Students.Include(m=>m.Schedules).Include(m=>m.Credit).Include(m=>m.Money).Include(m=>m.BalanceHistory).ThenInclude(m => m.CashFlow).ToListAsync();
            var managers = await _context.Managers.Include(m => m.Notifications).Include(m => m.BalanceHistory).ThenInclude(m=>m.CashFlow).ToListAsync();
            try
            {
                foreach (var item in tutors)
                {

                    _context.Entry(item).State = EntityState.Detached;
                    
                }
                foreach (var item in students)
                {

                    _context.Entry(item).State = EntityState.Detached;


                }
                foreach (var item in managers)
                {

                    _context.Entry(item).State = EntityState.Detached;


                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
         
            var allUsers = new List<User>();

            // Маппинг Tutors на UserDTO и добавление в список всех пользователей
            allUsers.AddRange(_mapper.Map<List<Tutor>>(tutors));

            // Маппинг Students на UserDTO и добавление в список всех пользователей
            allUsers.AddRange(_mapper.Map<List<Student>>(students));

            // Маппинг Managers на UserDTO и добавление в список всех пользователей
            allUsers.AddRange(_mapper.Map<List<Manager>>(managers));

            return allUsers;
            //var users = (List<UserDTO>)(await _context.Users.ToListAsync());

            //List<User> res;
            //try
            //{
            //    res = _mapper.Map<List<UserDTO>, List<User>>(users);
            //}
            //catch (Exception ex)
            //{

            //    throw;
            //}
            //return res;

            //var mapped = _mapper.Map<List<User>>(entities);
            //return mapped;
        }
        public async Task Remove(int id)
        {
            var res = await _context.Tutors.Include(m=>m.Schedules).FirstOrDefaultAsync(m=>m.UserId == id);
            _context.Entry(res).State = EntityState.Deleted;
            try
            {
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        //public async Task RemoveByFunc(Func<, bool>)
    
        public async Task<int> GetManagerId()
        {
            var manag = await _context.Managers.FirstOrDefaultAsync();
            _context.Entry(manag).State = EntityState.Detached;
            
            
            return manag.UserId;
        }
        public async Task<bool> AddUser(User user)
        {
            if (user is Student)
            {
                var mapped = _mapper.Map<StudentDTO>((Student)user);
                if (!_context.Students.Contains(mapped))
                {
                    await _context.Students.AddAsync(mapped);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }

            if (user is Tutor)
            {
                var mapped = _mapper.Map<TutorDTO>((Tutor)user);
                if (!_context.Tutors.Contains(mapped))
                {
                    await _context.Tutors.AddAsync(mapped);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }


            return true;
        }
     
        public async Task<bool> AddTonificationTokenToUser(NotificationTokens token, UserDTO user)
        {

            try
            {
                user.NotificationTokens.Add(token);
                //_context.Update(user);
                await _context.SaveChangesAsync();
                _context.Entry(user).State = EntityState.Detached;

            }
            catch (Exception ex)
            {

                throw ex;
            }
          
            return true;
        }


        public async Task RemoveTutorTime(UserDate date)
        {
            _context.UserDates.Remove(date);
            await _context.SaveChangesAsync(); 
        }
     
        public async Task UpdateTutorCourses(TutorDTO old)
        {
            var old_courses = _context.TutorCourses.Include(m=>m.Course).ThenInclude(m=>m.Goal).Where(m => m.TutorId == old.UserId).ToList();
            if (old.Courses.Count == 0 && old_courses.Count != 0)
            {
                foreach (var item in old_courses)
                {
                    _context.Entry(item).State = EntityState.Deleted;
                }
            }

            if(old.Courses.Count > 0 && old_courses.Count > old.Courses.Count)
            {
                foreach (var item in old_courses)
                {
                    var old2 = old.Courses.FirstOrDefault(m=>m.Course.Title == item.Course.Title);
                    if(old2 == null)
                    {
                        _context.Entry(item).State = EntityState.Deleted;
                        
                    }
                }
            }

            if (old_courses.Count  < old.Courses.Count)
            {
                foreach (var item in old.Courses)
                {
                    var new2 = old_courses.FirstOrDefault(m => m.Course.Title == item.Course.Title);
                    if (new2 == null)
                    {
                        try
                        {
                            _context.Entry(item).State = EntityState.Added;

                        }
                        catch (Exception ex)
                        {

                            throw ex;
                        }

                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public async Task SaveModel(UserDTO user)
        {
            if (user is StudentDTO || user.Role == "Student")
            {

                var st = (StudentDTO)user;
                _context.Entry(st).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                _context.Entry(st).State = EntityState.Detached;

            }
            if (user is TutorDTO || user.Role == "Tutor")
            {
                try
                {
                    var st = (TutorDTO)user;

                    await _context.SaveChangesAsync();
                    _context.Entry(st).State = EntityState.Detached;
                }
                catch (Exception ex)
                {

                    throw ex;
                }
             

            }
            if (user is ManagerDTO || user.Role == "Manager")
            {

                var st = (ManagerDTO)user;
                _context.Entry(st).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                _context.Entry(st).State = EntityState.Detached;


            }
        }
        public async Task SaveChanges(UserDTO userDTO)
        {
            try
            {
                await _context.SaveChangesAsync();
                _context.Entry(userDTO).State = EntityState.Detached;

            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }
        public async Task<StudentDTO> GetStudent(int userId)
        {
            return await _context.Students.Include(m=>m.Credit).Include(m=>m.Money).Include(m=>m.Schedules).FirstOrDefaultAsync(m=>m.UserId == userId);
        }
        public async Task Update(User user)
        {
            var dd = _context.ChangeTracker.Entries();
            try
            {
                if (user is Student)
                {

                    var st = _mapper.Map<StudentDTO>((Student)user);
                    //_context.Entry(st).State = EntityState.Modified;
                    _context.Students.Update(st);
                    await _context.SaveChangesAsync();

                    _context.Entry(st).State = EntityState.Detached;

                }
                if (user is Tutor)
                {

                    var st = _mapper.Map<TutorDTO>((Tutor)user);
                    //_context.Entry(st).State = EntityState.Modified;
                    _context.Tutors.Update(st);

                    await _context.SaveChangesAsync();
                    _context.Entry(st).State = EntityState.Detached;

                }
                if (user is Manager)
                {
                    var st = _mapper.Map<ManagerDTO>((Manager)user);

                   // _context.Entry(st).State = EntityState.Modified;

                    _context.Managers.Update(st);
                    await _context.SaveChangesAsync();

                    _context.Entry(st).State = EntityState.Detached;


                }
            }
            catch(DbUpdateConcurrencyException ex)
            {
                await SaveModel(_mapper.Map<UserDTO>(user));
            }
            catch (Exception ex)
            {
                Thread.Sleep(1000);
                var f = _context.ChangeTracker.Entries();

                foreach (var entry in f)
                {
                    if (entry.Entity != null && entry.State == EntityState.Unchanged)
                    {
                        entry.State = EntityState.Detached;
                    }
                }
                await Update(user);
            }
            
        }
        public async Task<UserDTO> GetUserByChatToken (string token)
        {
            try
            {
                var tutor = await _context.Tutors.Include(m => m.Notifications).Include(m => m.BalanceHistory).Include(m => m.Chat).Include(m => m.Chat.Messages).Include(m => m.Chat.Contacts).Include(m => m.Chat.ConnectionTokens).Include(m => m.Courses).Include(m => m.UserDates).FirstOrDefaultAsync(u => u.Chat.ConnectionTokens.Any(t => t.Token == token));
                if (tutor != null)
                {


                    return tutor;
                    //return _mapper.Map<Tutor>(tutor);
                }

                var student = await _context.Students.Include(m => m.Credit).Include(m => m.Notifications).Include(m => m.Money).Include(m => m.Chat).Include(m => m.Chat.Messages).Include(m => m.Chat.Contacts).Include(m => m.Chat.ConnectionTokens).Include(m => m.BalanceHistory).FirstOrDefaultAsync(u => u.Chat.ConnectionTokens.Any(t => t.Token == token));
                if (student != null)
                {


                    return student;
                    //return _mapper.Map<Student>(student);
                }

                var manager = await _context.Managers.Include(m => m.Chat).Include(m => m.Chat.Messages).Include(m => m.Chat.Contacts).Include(m => m.Chat.ConnectionTokens).Include(m => m.Notifications).FirstOrDefaultAsync(u => u.Chat.ConnectionTokens.Any(t => t.Token == token));
                if (manager != null)
                {


                    return manager;
                    //return _mapper.Map<Manager>(manager);
                }

                return null;

            }
            catch (Exception ex)
            {

                throw ex;
            }
          
        }


        public async Task<UserDTO> GetUser(int id)
        {
            return await _context.Users.Include(m=>m.NotificationTokens).Include(m=>m.BalanceHistory).ThenInclude(m=>m.CashFlow).Include(m=>m.Notifications).FirstOrDefaultAsync(m => m.UserId == id);
        }
        public async Task<TutorDTO> GetTutor(int id)
        {
            return await _context.Tutors.Include(m=>m.Schedules).ThenInclude(m=>m.Tasks).FirstOrDefaultAsync(m=>m.UserId == id);
        }
     
        public async Task<User> GetUserById(int id)
        {
            var gg = _context.ChangeTracker.Entries();

            //lock(_context){
            try
            {
                    var tutor = await _context.Tutors.Include(m => m.Schedules).Include(m => m.Notifications).Include(m=>m.BalanceHistory).ThenInclude(m=>m.CashFlow).Include(m => m.Chat).Include(m => m.Chat.Messages).Include(m => m.Chat.Contacts).Include(m => m.Chat.ConnectionTokens).Include(m => m.UserDates).Include(m=>m.Courses).ThenInclude(m=>m.Course).ThenInclude(m=>m.Goal).FirstOrDefaultAsync(m => m.UserId == (id));
                    if (tutor != null)
                    {
                    _context.Entry(tutor).State = EntityState.Detached;

                    return _mapper.Map<Tutor>(tutor);
                    }

                    var student = await _context.Students.Include(m=>m.Credit).Include(m=>m.Notifications).Include(m=>m.Money).Include(m=>m.Chat).Include(m=>m.Chat.Messages).Include(m=>m.Chat.Contacts).Include(m=>m.Chat.ConnectionTokens).Include(m=>m.BalanceHistory).ThenInclude(m => m.CashFlow).Include(m=>m.Schedules).ThenInclude(m=>m.Course).ThenInclude(m=>m.Goal).FirstOrDefaultAsync(m => m.UserId == id);
                    if (student != null)
                    {
                    _context.Entry(student).State = EntityState.Detached;


                    return _mapper.Map<Student>(student);
                    }

                    var manager = await _context.Managers.Include(m=>m.BalanceHistory).ThenInclude(m => m.CashFlow).Include(m => m.Chat).Include(m => m.Chat.Messages).Include(m => m.Chat.Contacts).Include(m => m.Chat.ConnectionTokens).Include(m => m.Notifications).FirstOrDefaultAsync(m => m.UserId == id);
                    if (manager != null)
                    {
                    _context.Entry(manager).State = EntityState.Detached;

                    return _mapper.Map<Manager>(manager);
                    }

                    return null;
                }
                catch (Exception ex)
                {

                    throw ex;
                }
           
            //}
                    }

        public async Task<User> GetUserByEmail(string email)
        {
            var tutor = await _context.Tutors.FirstOrDefaultAsync(x => x.Email == email);
            if (tutor != null)
            {

                _context.Entry(tutor).State = EntityState.Detached;
                
                return _mapper.Map<Tutor>(tutor);
            }

            var student = await _context.Students.FirstOrDefaultAsync(x => x.Email == email);
            if (student != null)
            {

                _context.Entry(student).State = EntityState.Detached;


                return _mapper.Map<Student>(student);
            }

            var manager = await _context.Managers.FirstOrDefaultAsync(x => x.Email == email);
            if (manager != null)
            {


                _context.Entry(manager).State = EntityState.Detached;
                return _mapper.Map<Manager>(manager);
            }

            return null;

        }
 
        public async Task<User> GetUserByToken(string id)
        {
            try
            {
                var tutor = await _context.Tutors.Include(m=>m.BalanceHistory).ThenInclude(m => m.CashFlow).Include(m => m.Schedules).ThenInclude(m => m.RescheduledLessons).Include(m => m.Schedules).ThenInclude(m => m.PaidLessons).Include(m => m.Schedules).ThenInclude(m=>m.ReadyDates).Include(m=>m.Schedules).ThenInclude(m=>m.SkippedDates).Include(m => m.Courses).ThenInclude(m => m.Course).ThenInclude(m => m.Goal).Include(m => m.Chat).Include(m => m.UserDates).FirstOrDefaultAsync(x => x.ActiveToken == id);
                if (tutor != null)
                {

                    //_context.Entry(tutor).State = EntityState.Detached;


                    var mapped = _mapper.Map<Tutor>((TutorDTO)tutor);
                    return mapped;
                }

                var student = await _context.Students.Include(m => m.Credit).Include(m => m.Chat).Include(m => m.Money).Include(m => m.BalanceHistory).ThenInclude(m => m.CashFlow).Include(m=>m.Schedules).ThenInclude(m=>m.Course).ThenInclude(m=>m.Goal).Include(m => m.Schedules).ThenInclude(m => m.RescheduledLessons).Include(m => m.Schedules).ThenInclude(m => m.PaidLessons).Include(m => m.Schedules).ThenInclude(m => m.ReadyDates).Include(m => m.Schedules).ThenInclude(m => m.SkippedDates).FirstOrDefaultAsync(x => x.ActiveToken == id);
                if (student != null)
                {

                    _context.Entry(student).State = EntityState.Detached;


                    return _mapper.Map<Student>((StudentDTO)student);
                }

                var manager = await _context.Managers.Include(m => m.BalanceHistory).ThenInclude(m => m.CashFlow).Include(m => m.Chat).FirstOrDefaultAsync(x => x.ActiveToken == id);
                if (manager != null)
                {


                    _context.Entry(manager).State = EntityState.Detached;

                    return _mapper.Map<Manager>((ManagerDTO)manager);
                }

                return null;
            }
            catch (Exception ex)
            {

                throw ex;
            }
               
            
            

        }
        public async Task<List<Notifications>> GetUserNotifications(int id)
        {
            var user = await GetUserById(id);

            return _mapper.Map<List<Notifications>>(user.Notifications);
        }
    }
}
