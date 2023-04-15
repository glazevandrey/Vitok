using System.Threading.Tasks;

namespace web_server.Services.Interfaces
{
    public interface ISenderService
    {
        public Task SendMessage(int id, string message);
    }
}
