using System.Threading.Tasks;
using web_server.Services.Interfaces;

namespace web_server.Quartz
{
    public interface IQuartzService
    {
        Task MainParse(ISenderService _senderService, IScheduleService _scheduleService);
    }
}
