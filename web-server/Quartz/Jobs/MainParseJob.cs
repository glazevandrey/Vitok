using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Threading.Tasks;
using web_server.Services.Interfaces;

namespace web_server.Quartz.Jobs
{
    public class MainParseJob : IJob
    {
        private readonly IQuartzService _quartzService;
        private readonly IServiceCollection _services;
        ISenderService _senderService;
        IScheduleService _scheduleService;
        public MainParseJob(IQuartzService quartzService, ISenderService senderService, IScheduleService scheduleService)
        {
            _senderService = senderService;
            _scheduleService = scheduleService;
            _quartzService = quartzService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            await _quartzService.MainParse(_senderService, _scheduleService);
        }
    }
}
