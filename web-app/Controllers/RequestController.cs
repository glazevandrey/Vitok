using Microsoft.AspNetCore.Mvc;
using System;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/requesttimes")]
    public class RequestController : Controller
    {
        public string Index()
        {
            var text = "";
            foreach (var item in Program.requestTimes)
            {
                text += (item.url + " : " + item.time);
                text += Environment.NewLine;
            }

            return text;
        }
    }
}
