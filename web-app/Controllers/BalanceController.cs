using Microsoft.AspNetCore.Mvc;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using web_server.DbContext;
using web_server.Models;
using web_server.Services;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/balance")]
    public class BalanceController : Controller
    {
        IJsonService _jsonService;
        IRequestService _requestService;
        public BalanceController(IJsonService jsonService, IRequestService requestService)
        {
            _jsonService = jsonService;
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
            ViewData["role"] = user.Role;
            ViewData["tariffs"] = TestData.Tariffs;
            ViewData["usertoken"] = user.UserId;
            ViewData["lessons"] = user.LessonsCount;
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;

            return View(user);
        }

        [HttpPost("setLessons", Name = "setLessons")]
        public IActionResult SetLessons([FromForm] string userId, [FromForm] string count, [FromForm] string isTrialPay)
        {
            CustomRequestPost req = new CustomRequestPost("api/account/addlessons", $"{userId};{count};{isTrialPay}");
            _requestService.SendPost(req, HttpContext);

            return RedirectToAction("Index", "Balance");
        }
    }
}
