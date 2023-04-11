using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.Models.DBModels;

namespace web_server.Database.Repositories
{
    public class ContactsRepository
    {
        DataContext _context;
        public ContactsRepository(DataContext context) 
        {
            _context = context;
        }

        public async Task<List<Contact>> GetContacts()
        {
           return await _context.Contacts.ToListAsync();
        }
    }
}
