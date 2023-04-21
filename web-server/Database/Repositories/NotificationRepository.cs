﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using web_server.Models;
using web_server.Models.DTO;

namespace web_server.Database.Repositories
{
    public class NotificationRepository
    {
        DataContext _context;
        IMapper _mapper;
        public NotificationRepository(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<bool> AddNotification(Notifications notif)
        {
            var result = await _context.Notifications.AddAsync(_mapper.Map<NotificationsDTO>(notif));
            return true;
        }
        public async Task<NotificationsDTO> GetNotification(int id)
        {
            var res = await _context.Notifications.FirstOrDefaultAsync(m => m.Id == id);
            //_context.Entry(res).State = EntityState.Detached;

            return res;
        }
        public async Task<bool> UpdateNotification()
        {
            //var user = await _context.Users.Include(m=>m.Notifications).FirstOrDefaultAsync(m=>m.Notifications.FirstOrDefault(m=>m.DateTime == notification.DateTime) != null);
            //user.Notifications.FirstOrDefault(m => m.DateTime == notification.DateTime).Readed = true;
            //_context.Users.Update(_mapper.Map<UserDTO>(user));
            //context.Notifications.Update(_mapper.Map<NotificationsDTO>(notification));

            await _context.SaveChangesAsync();
            //_context.Entry(_mapper.Map<UserDTO>(user)).State = EntityState.Detached;
            return true;
        }
    }
}