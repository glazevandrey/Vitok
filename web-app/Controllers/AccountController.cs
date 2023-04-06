using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using web_server.Models;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/account")]
    public class AccountController : Controller
    {
        IRequestService _requestService;

        public AccountController(IRequestService requestService)
        {
            _requestService = requestService;
        }
        [HttpGet("logout", Name = "logout")]

        public IActionResult Logout()
        {
            if (HttpContext.Request.Cookies.ContainsKey(".AspNetCore.Application.Id"))
            {
                HttpContext.Response.Cookies.Delete(".AspNetCore.Application.Id");
            }
            return Redirect("/login");
        }

        [HttpGet]
        public IActionResult Index([FromQuery] string error = null)
        {
            CustomRequestGet request = new GetUserByToken(Request.Cookies[".AspNetCore.Application.Id"]);
            var result = _requestService.SendGet(request, HttpContext);

            if (!result.success)
            {
                return Redirect("/account/logout");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(result.result.ToString());
            ViewData["usertoken"] = user.UserId;
            ViewData["userid"] = user.UserId;
           
            ViewData["lessons"] = user.LessonsCount;
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;

            if (error != null)
            {
                ViewData["error"] = error;
            }
            return View(user);
        }

        [HttpPost("saveinfo", Name = "saveinfo")]
        public IActionResult SaveInfo([FromForm] User user)
        {
            if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName))
            {
                return RedirectToAction("Index", "Account", new { error = "Неудачная попытка сохранить данные" });
            }

            user.FirstName = user.FirstName.Trim();
            user.LastName = user.LastName.Trim();

            CustomRequestPost req = new CustomRequestPost("api/account/saveuserinfo", user);
            var response = _requestService.SendPost(req, HttpContext);
            if (response == null)
            {
                return RedirectToAction("Index", "Account", new { error = "Неудачная попытка сохранить данные" });
            }

            return Redirect("/account");
        }


    }
}
