using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/requesttimes")]
    public class RequestController : Controller
    {
        public string Index()
        {
            using (var stream = new StreamWriter("/time.txt"))
            {
                foreach (var item in Program.requestTimes)
                {
                    stream.WriteLine(item.url + " : " + item.time);
                }
            }

            var text = "";
            using (var stream = new StreamReader("/time.txt"))
            {
                text = stream.ReadToEnd();
            }
            return text;
        }
    }
}
