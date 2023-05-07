using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using web_app.Requests;
using web_app.Services;
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
        public IActionResult LoginUser([FromForm] string email, [FromForm] string password, [FromQuery] string id)
        {
            if (email == null || password == null)
            {
                return RedirectToAction("login", new { error = "Необходимо заполнить оба поля" });
            }

            CustomRequestPost req = null;
            Guid guid;
            if(!Guid.TryParse(id, out guid))
            {
                req = new CustomRequestPost("api/home/LoginUser", $"{email};{password}");
            }
            else
            {
                req = new CustomRequestPost("api/home/LoginUser", $"{email};{password};{guid}");

            }
            var response = _requestService.SendPost(req, HttpContext);
            if (response == null || response.result == null)
            {
                return RedirectToAction("login", new { error = "Неудачная попытка входа" });
            }
            if (!response.success)
            {
                return RedirectToAction("login", new { error = response.result });
            }

            Dictionary<string, string> keys = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(response.result.ToString());
            HttpContext.Response.Cookies.Append(".AspNetCore.Application.Id", keys.First().Key.ToString(),
            new CookieOptions
            {
                // TODO: сколько нужно?
                MaxAge = TimeSpan.FromMinutes(1160)
            });

            HttpContext.Request.Headers.Add(".AspNetCore.Application.Id", keys.First().Key.ToString());


            if (keys.First().Value.ToString() == "Manager")
            {
                return Redirect("/manageschool");

            }
            else
            {
                return Redirect("/schedule");

            }
            //return RedirectToAction("login", new { role = keys.First().Value.ToString() });


        }
    }
}
