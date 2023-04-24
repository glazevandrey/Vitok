using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace web_app
{
    public class RequestTime
    {
        public string url { get; set; }
        public TimeSpan time { get; set; }
    }
    public class Program
    {
        public static List<RequestTime> requestTimes = new List<RequestTime>();
        public static HubConnection Conn;
        public static string web_server_ip = "";
        public static JsonSerializerSettings settings = new JsonSerializerSettings();
        public static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");
            settings.Converters.Add(new web_app.UserConverter());

            web_server_ip = "http://localhost:23382";

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://localhost:23571");
                });
    }
}
