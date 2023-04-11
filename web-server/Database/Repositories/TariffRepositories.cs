using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using web_server.Models.DBModels;

namespace web_server.Database.Repositories
{
    public class TariffRepositories
    {
        DataContext _context;
        public TariffRepositories(DataContext context)
        {
            _context = context;
        }

        public async Task<Tariff> GetTariffByLessonsCount(int count)
        {
            return await _context.Tariffs.FirstOrDefaultAsync(m => m.LessonsCount == count);
        } 
    }
}
