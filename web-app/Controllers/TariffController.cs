using Microsoft.AspNetCore.Mvc;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using web_server.DbContext;
using web_server.Models;

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
            CustomRequestGet req = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res = _requestService.SendGet(req, HttpContext);
            if (!res.success)
            {
                return Redirect("/login");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString());
            ViewData["usertoken"] = user.UserId;
            ViewData["role"] = user.Role;
            ViewData["lessons"] = user.LessonsCount;
            ViewData["count"] = user.LessonsCount;

            return View(TestData.Tariffs);
        }
    }
}
