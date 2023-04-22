using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using web_server.Models;
using web_server.Models.DTO;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
  

        private readonly IServiceScopeFactory _scopeFactory;

        public NotificationBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                Thread.Sleep(10000);
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                       
                    }
                    catch (Exception ex)
                    {
                        Program.BackInAir = false;

                    }
                }

            });

        }
    }
}
