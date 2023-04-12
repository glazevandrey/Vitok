using Microsoft.AspNetCore.Mvc;
using web_app.Requests.Get;
using web_app.Requests;
using web_app.Services;
using web_server.DbContext;
using web_server.Services.Interfaces;
using web_server.Models.DBModels;
using System.Collections.Generic;

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
        public IActionResult Index([FromQuery] string error = null)
        {
            CustomRequestGet req = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res = _requestService.SendGet(req, HttpContext);
            if (!res.success)
            {
                return Redirect("/login");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString(), Program.settings);
            var req3 = new GetTariffsRequest();
            var res3 = _requestService.SendGet(req3);
            var tariffs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Tariff>>(res3.result.ToString());



            ViewData["role"] = user.Role;
            ViewData["tariffs"] = tariffs;
            ViewData["usertoken"] = user.UserId;

            if(user.Role == "Student")
            {
                ViewData["lessons"] = ((Student)user).LessonsCount;
            }

            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;
            if (user.FirstLogin == true && user.Role == "Student")
            {
                ViewData["firstLogin"] = true;
            }
            if (error != null)
            {
                ViewData["error"] = error;
            }
            return View(user);
        }

        [HttpPost("setLessons", Name = "setLessons")]
        public IActionResult SetLessons([FromForm] string userId, [FromForm] string count, [FromForm] string isTrialPay, [FromForm] string count3)
        {
            if(count == 0.ToString() && count3 == 0.ToString())
            {
                return RedirectToAction("Index", "Balance", new { error = "Нельзя пополнить счет на 0 занятий" });

            }
            CustomRequestPost req = new CustomRequestPost("api/account/addlessons", $"{userId};{(count == "0" ? count3 : count)};{isTrialPay}");
            _requestService.SendPost(req, HttpContext);

            return RedirectToAction("Index", "Balance");
        }

        [HttpPost("withdrawbalance", Name = "withdrawbalance")]
        public IActionResult WithdrawBalance([FromForm] string tutorId, [FromForm] string count, [FromForm] string count3)
        {
            if (string.IsNullOrEmpty(count) || count[0] == '0')
            {
                return RedirectToAction("Index", "Balance");
            }

            CustomRequestPost req = new CustomRequestPost("api/account/tutorWithdraw", $"{tutorId};{count}");
            var res = _requestService.SendPost(req, HttpContext);
            if (!res.success)
            {
                return RedirectToAction("Index", "Balance", new { error = "Недостаточно средств для вывода." });
            }
            return RedirectToAction("Index", "Balance");
        }

    }
}
