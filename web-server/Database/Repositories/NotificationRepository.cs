using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using web_server.Models;

namespace web_server.Database.Repositories
{
    public class NotificationRepository
    {
        DataContext _context;
        public NotificationRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> AddNotification(Notifications notif)
        {
            var result = await _context.Notifications.AddAsync(notif);
            return true;
        }
        public async Task<Notifications> GetNotification(int id)
        {
            var result = await _context.Notifications.FirstOrDefaultAsync(m=>m.Id == id);
            return result;
        }
        public async Task<bool> UpdateNotification(Notifications notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}