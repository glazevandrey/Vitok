using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
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

        public async Task<NotificationsDTO> GetNotification(int id)
        {
            var res = await _context.Notifications.FirstOrDefaultAsync(m => m.Id == id);
            return res;
        }
        public async Task<bool> UpdateNotification()
        {
            await _context.SaveChangesAsync();
            return true;
        }
    }
}