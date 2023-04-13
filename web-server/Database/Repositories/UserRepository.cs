using AutoMapper;
using AutoMapper.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
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
            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<Registration> GetRegistrationByUserId(int userId)
        {
            var reg = await _context.Registrations.FirstOrDefaultAsync(m=>m.UserId == userId);
            return reg;
        }
        public async Task<List<Tutor>> GetAllTutors()
        {
            var model = await _context.Tutors.Include(m => m.Schedules).Include(m => m.UserDates).Include(m => m.BalanceHistory).Include(m => m.Courses).ToListAsync();
            return _mapper.Map<List<Tutor>>(model);
        }
        public async Task<List<User>> GetAll()
        {

            var tutors = await _context.Tutors.Include(m => m.Notifications).Include(m => m.Schedules).Include(m=>m.UserDates).Include(m=>m.BalanceHistory).Include(m=>m.Courses).ToListAsync();
            var students = await _context.Students.Include(m => m.Notifications).Include(m=>m.Schedules).Include(m=>m.Credit).Include(m=>m.Money).Include(m=>m.BalanceHistory).ToListAsync();
            var managers = await _context.Managers.Include(m => m.Notifications).ToListAsync();
            try
            {
                foreach (var item in tutors)
                {
                    
                    
                    
                }
                foreach (var item in students)
                {
                    
                    
                    
                }
                foreach (var item in managers)
                {
                    
                      
                    
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
            var res = await _context.Users.FirstOrDefaultAsync(m=>m.UserId == id);
            _context.Entry(res).State = EntityState.Detached;
            _context.Remove(res);
            await _context.SaveChangesAsync();
        }
        //public async Task RemoveByFunc(Func<, bool>)
        public async Task<Tutor> SetTutorFreeDate(int tutorId, DateTime date)
        {
            var tutor = await _context.Tutors.FindAsync(tutorId);
            tutor.UserDates.Add(new UserDate() { dateTime = date });
            _context.Update(tutor);
            await _context.SaveChangesAsync();

            return _mapper.Map<Tutor>(tutor);
        }
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
     
        public async Task<bool> AddTonificationTokenToUser(NotificationTokens token, int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(m=>m.UserId == userId);
            
            
            
            user.NotificationTokens.Add(token);
            _context.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeNotifTokenStatus(string status, string connectionId, int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(m=>m.UserId == userId);
            
            
            
            if (user == null || user.NotificationTokens == null || user.NotificationTokens.Count == 0)
            {
                return false;
            }
            user.NotificationTokens.FirstOrDefault(m => m.TokenKey == connectionId).TokenValue = status;
            _context.Update(user);

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task Update(User user)
        {
                if (user is Student)
                {

                var st = _mapper.Map<StudentDTO>((Student)user);
                _context.Students.Update(st);

            }
            if (user is Tutor)
                {

                var st = _mapper.Map<TutorDTO>((Tutor)user);

                _context.Tutors.Update(st);
                

            }
            if (user is Manager)
                {
                var st = _mapper.Map<ManagerDTO>((Manager)user);
                

                _context.Managers.Update(st);
                

            }
            try
            {
                var fg = _context.ChangeTracker.Entries();

                await _context.SaveChangesAsync();
                _context.Entry(_mapper.Map<UserDTO>(user)).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                var fg = _context.ChangeTracker.Entries();

                throw ex;
            }
        }
        public async Task<User> GetUserByChatToken (string token)
        {
            try
            {
                var tutor = await _context.Tutors.Include(m => m.Notifications).Include(m => m.BalanceHistory).Include(m => m.Chat).Include(m => m.Chat.Messages).Include(m => m.Chat.Contacts).Include(m => m.Chat.ConnectionTokens).Include(m => m.Courses).Include(m => m.UserDates).FirstOrDefaultAsync(u => u.Chat.ConnectionTokens.Any(t => t.Token == token));
                if (tutor != null)
                {
                    
                    
                    
                    return _mapper.Map<Tutor>(tutor);
                }

                var student = await _context.Students.Include(m => m.Credit).Include(m => m.Notifications).Include(m => m.Money).Include(m => m.Chat).Include(m => m.Chat.Messages).Include(m => m.Chat.Contacts).Include(m => m.Chat.ConnectionTokens).Include(m => m.BalanceHistory).FirstOrDefaultAsync(u => u.Chat.ConnectionTokens.Any(t => t.Token == token));
                if (student != null)
                {
                    
                    
                    
                    return _mapper.Map<Student>(student);
                }

                var manager = await _context.Managers.Include(m => m.Chat).Include(m => m.Chat.Messages).Include(m => m.Chat.Contacts).Include(m => m.Chat.ConnectionTokens).Include(m => m.Notifications).FirstOrDefaultAsync(u => u.Chat.ConnectionTokens.Any(t => t.Token == token));
                if (manager != null)
                {
                    
                    
                    
                    return _mapper.Map<Manager>(manager);
                }

                return null;

            }
            catch (Exception ex)
            {

                throw ex;
            }
          
        }
        public async Task<User> GetUserById(int id)
        {

            //lock(_context){
            try
            {
                    var tutor = await _context.Tutors.Include(m => m.Schedules).Include(m => m.Notifications).Include(m=>m.BalanceHistory).Include(m => m.Chat).Include(m => m.Chat.Messages).Include(m => m.Chat.Contacts).Include(m => m.Chat.ConnectionTokens).Include(m => m.Courses).Include(m => m.UserDates).FirstOrDefaultAsync(m => m.UserId == (id));
                    if (tutor != null)
                    {
                    
                    
                    
                    return _mapper.Map<Tutor>(tutor);
                    }

                    var student = await _context.Students.Include(m=>m.Credit).Include(m=>m.Notifications).Include(m=>m.Money).Include(m=>m.Chat).Include(m=>m.Chat.Messages).Include(m=>m.Chat.Contacts).Include(m=>m.Chat.ConnectionTokens).Include(m=>m.BalanceHistory).Include(m=>m.Schedules).FirstOrDefaultAsync(m => m.UserId == id);
                    if (student != null)
                    {
                    
                    
                    
                    return _mapper.Map<Student>(student);
                    }

                    var manager = await _context.Managers.Include(m => m.Chat).Include(m => m.Chat.Messages).Include(m => m.Chat.Contacts).Include(m => m.Chat.ConnectionTokens).Include(m => m.Notifications).FirstOrDefaultAsync(m => m.UserId == id);
                    if (manager != null)
                    {
                    
                    
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
                
                
                
                return _mapper.Map<Tutor>(tutor);
            }

            var student = await _context.Students.FirstOrDefaultAsync(x => x.Email == email);
            if (student != null)
            {
                
                try
                {
                    

                }
                catch (Exception ex)
                {

                    throw ex;
                }
                

                return _mapper.Map<Student>(student);
            }

            var manager = await _context.Managers.FirstOrDefaultAsync(x => x.Email == email);
            if (manager != null)
            {
                
                
                
                return _mapper.Map<Manager>(manager);
            }

            return null;

        }
 
        public async Task<User> GetUserByToken(string id)
        {
            
                var tutor =  await _context.Tutors.Include(m=>m.Schedules).Include(m=>m.Courses).Include(m => m.Chat).Include(m=>m.UserDates).FirstOrDefaultAsync(x => x.ActiveToken == id);
                if (tutor != null)
                {
                
                
                

                return _mapper.Map<Tutor>(tutor);
                }

                var student =  await _context.Students.Include(m => m.Credit).Include(m => m.Chat).Include(m => m.Money).Include(m => m.BalanceHistory).Include(m => m.Schedules).FirstOrDefaultAsync(x => x.ActiveToken == id);
                if (student != null)
                {
                
                
                
                return _mapper.Map<Student>(student);
                }

                var manager =  await _context.Managers.Include(m => m.Chat).FirstOrDefaultAsync(x => x.ActiveToken == id);
                if (manager != null)
                {
                
                
                
                return _mapper.Map<Manager>(manager);
                }

                return null;
            
            

        }
        public async Task<List<Notifications>> GetUserNotifications(int id)
        {
            var user = await GetUserById(id);

            return _mapper.Map<List<Notifications>>(user.Notifications);
        }
    }
}
