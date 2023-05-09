using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using web_app.Requests;
using web_app.Requests.Get;
using web_app.Services;
using web_server.Models.DBModels;

namespace web_app.Controllers
{
    public class TariffController : Controller
    {
        IRequestService _requestService;
        public TariffController(IRequestService requestService)
        {
            _requestService = requestService;
        }
        public IActionResult Index()
        {
            CustomRequestGet req = new GetLiteUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res = _requestService.SendGet(req, HttpContext);
            if (!res.success)
            {
                return Redirect("/login");
            }

            var user = (Student)Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString(), Program.settings);
            ViewData["usertoken"] = user.UserId;
            ViewData["role"] = user.Role;

            if (user.Role == "Student")
            {
                ViewData["lessons"] = ((Student)user).LessonsCount;
            }
            ViewData["count"] = user.LessonsCount;
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;
            if (user.Role == "Student")
            {
                ViewData["firstLogin"] = ((Student)user).FirstLogin;
            }

            var req2 = new GetTariffsRequest();
            var res2 = _requestService.SendGet(req2);
            var tariffs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Tariff>>(res2.result.ToString());

            tariffs = tariffs.OrderBy(m => m.LessonsCount).ToList();
            tariffs.Reverse();
            return View(tariffs);
        }
    }
}
