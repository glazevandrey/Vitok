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
            var model = await _context.Tutors.ToListAsync();
            return _mapper.Map<List<Tutor>>(model);
        }
        public async Task<List<User>> GetAll()
        {

            var tutors = await _context.Tutors.AsNoTracking().ToListAsync();
            var students = await _context.Students.AsNoTracking().ToListAsync();
            var managers = await _context.Managers.AsNoTracking().ToListAsync();

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
            var res = await _context.Users.FirstAsync(m=>m.UserId == id);
            _context.Remove(res);
            await _context.SaveChangesAsync();
        }
        //public async Task RemoveByFunc(Func<, bool>)
        public async Task<Tutor> SetTutorFreeDate(int tutorId, DateTime date)
        {
            var tutor = await _context.Tutors.FindAsync(tutorId);
            tutor.UserDates.dateTimes.Add(date);
            _context.Update(tutor);
            await _context.SaveChangesAsync();

            return _mapper.Map<Tutor>(tutor);
        }
        public async Task<int> GetManagerId()
        {
            return (await _context.Managers.FirstOrDefaultAsync()).UserId;
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
        public async Task<bool> AddNotification(Notifications notif)
        {
           
                await _context.Notifications.AddAsync(notif);
                await _context.SaveChangesAsync();
                return true;
            

        }

        public async Task<bool> AddTonificationTokenToUser(NotificationTokens token, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            user.NotificationTokens.Add(token);
            _context.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeNotifTokenStatus(string status, string connectionId, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            user.NotificationTokens.FirstOrDefault(m => m.TokenKey == connectionId).TokenValue = status;
            _context.Update(user);

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task Update(User user)
        {


            if (user is Student)
            {                
                _context.Students.Update(_mapper.Map<StudentDTO>((Student)user));
            }
            if(user is Tutor)
            {

                _context.Tutors.Update(_mapper.Map<TutorDTO>((Tutor)user));
            }

            await _context.SaveChangesAsync();
        }
        //public async Task Update(UserDTO user)
        //{
        //    if (user is StudentDTO)
        //    {
        //        _context.Students.Update((StudentDTO)user);
        //    }
        //    if (user is TutorDTO)
        //    {
        //        _context.Tutors.Update((TutorDTO)(user));
        //    }

        //    await _context.SaveChangesAsync();
        //}
        public async Task<User> GetUserById(int id)
        {
            lock(_context){
                var tutor = _context.Tutors.AsNoTracking().FirstOrDefaultAsync(m => m.UserId == (id)).Result;
                if (tutor != null)
                {
                    return _mapper.Map<Tutor>(tutor);
                }

                var student =  _context.Students.AsNoTracking().FirstOrDefaultAsync(m=>m.UserId == id).Result;
                if (student != null)
                {
                    return _mapper.Map<Student>(student);
                }

                var manager =  _context.Managers.AsNoTracking().FirstOrDefaultAsync( m=>m.UserId == id).Result;
                if (manager != null)
                {
                    return _mapper.Map<Manager>(manager);
                }

                return null;
            }
            

        }

        public async Task<User> GetUserByEmail(string email)
        {
            var tutor = await _context.Tutors.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
            if (tutor != null)
            {
                return _mapper.Map<Tutor>(tutor);
            }

            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
            if (student != null)
            {

                return _mapper.Map<Student>(student);
            }

            var manager = await _context.Managers.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
            if (manager != null)
            {
                return _mapper.Map<Student>(manager);
            }

            return null;

        }
        //public async Task<User> GetUserByEmail(string email)
        //{
        //    var tutor = await _context.Tutors.FirstOrDefaultAsync(x => x.Email == email);
        //    if (tutor != null)
        //    {
        //        return _mapper.Map<Tutor>(tutor);
        //    }

        //    var student = await _context.Students.FirstOrDefaultAsync(x => x.Email == email);
        //    if (student != null)
        //    {
        //        return _mapper.Map<Student>(student);
        //    }

        //    var manager = await _context.Managers.FirstOrDefaultAsync(x => x.Email == email);
        //    if (manager != null)
        //    {
        //        return _mapper.Map<Manager>(manager);
        //    }

        //    return null;

        //}
        public async Task<User> GetUserByToken(string id)
        {
            var tutor = await _context.Tutors.FirstOrDefaultAsync(x => x.ActiveToken == id);
            if (tutor != null)
            {
                return _mapper.Map<Tutor>(tutor);
            }

            var student = await _context.Students.FirstOrDefaultAsync(x => x.ActiveToken == id);
            if (student != null)
            {
                return _mapper.Map<Student>(student);
            }

            var manager = await _context.Managers.FirstOrDefaultAsync(x => x.ActiveToken == id);
            if (manager != null)
            {
                return _mapper.Map<Manager>(manager);
            }

            return null;

        }
        public async Task<List<Notifications>> GetUserNotifications(int id)
        {
            return (await _context.Notifications.Where(m => m.UserIdTo == id).ToListAsync());
        }
    }
}
