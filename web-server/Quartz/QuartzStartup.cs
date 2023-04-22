using web_server.Quartz.Jobs;
using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using System;

namespace web_server.Quartz
{
    public class QuartzStartup
    {
        private IScheduler _scheduler; // После запуска и до завершения выключения ссылается на объект планировщика
        private readonly IServiceProvider _container;

        public QuartzStartup(IServiceProvider container)
        {
            _container = container;
        }
        public static async void Start(IServiceProvider container, IConfiguration configuration)
        {
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            scheduler.JobFactory = new JobFactory(container);
            await scheduler.Start();

            // Работа по повторной отправке аутентификационных данных
            var job = JobBuilder.Create<MainParseJob>()
                .WithIdentity("MainParseJob", "group1")
                .Build();
            var trigger = TriggerBuilder.Create()
                .WithIdentity("MainParseJob", "group1")
                .StartAt(DateTimeOffset.Now.AddSeconds(15))
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(2) // Повторяем каждый час
                    .RepeatForever())
                .Build();
            await scheduler.ScheduleJob(job, trigger);

        }
        public void Stop()
        {
            if (_scheduler == null)
            {
                return;
            }

            // give running jobs 30 sec (for example) to stop gracefully
            if (_scheduler.Shutdown(waitForJobsToComplete: true).Wait(30000))
            {
                _scheduler = null;
            }
            else
            {
                // jobs didn't exit in timely fashion - log a warning...
            }
        }
    }
}
