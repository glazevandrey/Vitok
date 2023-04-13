using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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
        public async Task<Notifications> GetNotification(int id)
        {
            var result = _mapper.Map<Notifications>(await _context.Notifications.AsNoTracking().FirstOrDefaultAsync(m=>m.Id == id));
            return result;
        }
        public async Task<bool> UpdateNotification(Notifications notification)
        {
            _context.Notifications.Update(_mapper.Map<NotificationsDTO>(notification));
            await _context.SaveChangesAsync();
            return true;
        }
    }
}