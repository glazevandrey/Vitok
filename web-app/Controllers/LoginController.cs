using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using web_app.Requests.Get;
using web_app.Requests;
using web_app.Services;
using web_server.Services.Interfaces;
using web_server.Models.DBModels;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/login")]
    public class LoginController : Controller
    {
        IJsonService _jsonService;
        IRequestService _requestService;
        public LoginController(IJsonService jsonService, IRequestService requestService)
        {
            _jsonService = jsonService;
            _requestService = requestService;
        }
        [HttpGet]
        public IActionResult Login([FromQuery] string id, [FromQuery] string error, [FromQuery] string role)
        {
            if (id != null)
            {
                ViewData["Id"] = id;
            }

            if (Request.Cookies.ContainsKey(".AspNetCore.Application.Id"))
            {
                if (role == "Manager")
                {
                    return Redirect("/manageschool");

                }
                else
                {
                    return Redirect("/schedule");
                }
            }

            if (error != null)
            {
                ViewData["error"] = error;
            }


            return View();
        }

        [HttpPost]
        public IActionResult LoginUser([FromForm] User user)
        {
            if (user.Email == null || user.Password == null)
            {
                return RedirectToAction("login", new { error = "Необходимо заполнить оба поля" });
            }

            CustomRequestPost req = new CustomRequestPost("api/home/LoginUser", user);
            var response = _requestService.SendPost(req, HttpContext);
            if (!response.success || response == null)
            {
                return RedirectToAction("login", new { error = response.result });
            }

            HttpContext.Response.Cookies.Append(".AspNetCore.Application.Id", response.result.ToString(),
            new CookieOptions
            {
                // TODO: сделать больше
                MaxAge = TimeSpan.FromMinutes(1160)
            });

            HttpContext.Request.Headers.Add(".AspNetCore.Application.Id", response.result.ToString());

            var req2 = new GetUserByToken(response.result.ToString());
            var res2 = _requestService.SendGet(req2, HttpContext);
            try
            {
                user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res2.result.ToString());
            }
            catch (Exception)
            {
                return RedirectToAction("login");
            }
            return RedirectToAction("login", new { role = user.Role });


        }
    }
}
