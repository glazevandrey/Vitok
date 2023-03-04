using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading;
using web_server.Models;

namespace web_server
{
    public class Program
    {
        public static Dictionary<int, Timer> Timers = new Dictionary<int, Timer>();
        public static Dictionary<string, List<User>> ChatUsers = new Dictionary<string, List<User>>();
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
