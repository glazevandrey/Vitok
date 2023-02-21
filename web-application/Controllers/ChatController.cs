using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using System;

namespace web_application.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            //string server = "http://localhost:23571";
            //string relativePath = "messageHub";
            //Uri serverUri = new Uri(server);

            //// needs UriKind arg, or UriFormatException is thrown
            //Uri relativeUri = new Uri(relativePath, UriKind.Relative);
            //Uri fullUri = new Uri(serverUri, relativeUri);

            //var connection = new Microsoft.AspNetCore.SignalR.Client.HubConnectionBuilder().WithUrl(fullUri).Build();
            //try
            //{
            //    connection.StartAsync().Wait();

            //}
            //catch (Exception ex)
            //{

            //    throw;
            //}

            return View();
        }
    }
}
