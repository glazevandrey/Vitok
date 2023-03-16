using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using web_app.Models.Requests;
using web_app.Services;
using web_server.Models;
using web_server.Services.Interfaces;

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
        public IActionResult Login([FromQuery] string id, [FromQuery] string error)
        {
            if (id != null)
            {
                ViewData["Id"] = id;
            }

            if (Request.Cookies.ContainsKey(".AspNetCore.Application.Id"))
            {
                return Redirect("/account");
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
            if (!response.success)
            {
                return RedirectToAction("login", new { error = response.result });
            }

            HttpContext.Response.Cookies.Append(".AspNetCore.Application.Id", response.result.ToString(),
            new CookieOptions
            {
                // TODO: сделать больше
                MaxAge = TimeSpan.FromMinutes(60)
            });

            return Redirect("/login");
        }
    }
}
