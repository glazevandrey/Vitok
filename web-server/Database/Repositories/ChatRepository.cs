using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using web_server.Models.DBModels;

namespace web_server.Database.Repositories
{
    public class ChatRepository
    {
        DataContext _context;
        public ChatRepository(DataContext context)
        {
            _context = context;
        }

        public  async Task<ChatUser> GetChatUserByUserId(int userId)
        {
            return await _context.ChatUsers.Include(m=>m.InChat).Include(m=>m.Messages).Include(m=>m.ConnectionTokens).Include(m=>m.Contacts).FirstOrDefaultAsync(m=>m.UserId == userId);
        }
        public async Task<int> AddChatUser(ChatUser user)
        {
            try
            {
                await _context.ChatUsers.AddAsync(user);
                var res = await _context.SaveChangesAsync();
                return res;

            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
        public async Task<ChatUser> GetChatUserByFunc(Func<ChatUser, bool> func)
        {
            return _context.ChatUsers.Include(m=>m.ConnectionTokens).Include(m=>m.Messages).Include(m=>m.Contacts).FirstOrDefault(func);
        }
        public async Task<bool> Update(ChatUser user)
        {
            _context.ChatUsers.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
