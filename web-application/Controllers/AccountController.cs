using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using server.Models;
using web_application.Models.Requests;
using web_application.Models.Requests.Get;
using web_application.Services;

namespace web_application.Controllers
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
        public IActionResult Index()
        {
            CustomRequestGet request = new GetUserByToken(Request.Cookies[".AspNetCore.Application.Id"]);
            var result = _requestService.SendGet(request, HttpContext);

            if (!result.success)
            {
                return Redirect("/account/logout");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(result.result.ToString());
            return View(user);
        }

        [HttpPost("saveinfo", Name = "saveinfo")]
        public IActionResult SaveInfo([FromForm] User user)
        {
            CustomRequestPost req = new CustomRequestPost("api/account/saveuserinfo", user);
            var response = _requestService.SendPost(req, HttpContext);
            if (response == null)
            {
                return BadRequest("Неудачная попытка сохранить данные");
            }

            return Redirect("/account");
        }
    }
}
