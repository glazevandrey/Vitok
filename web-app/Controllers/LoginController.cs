using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using web_server.Models;
using web_server.Services;

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
        public IActionResult Login([FromQuery] string id)
        {
            if (id != null)
            {
                ViewData["Id"] = id;

            }

            if (Request.Cookies.ContainsKey(".AspNetCore.Application.Id"))
            {

                CustomRequestGet request = new GetUserByToken(Request.Cookies[".AspNetCore.Application.Id"]);
                var result = _requestService.SendGet(request);

                //if (!result.success)
                //{
                //    return View();
                //}
                return Redirect("/account");

                //ViewData["Auth"] = true;
                //var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(result.result.ToString());
                //if (result != null)
                //{
                //}
            }

            return View();
        }

        [HttpPost]
        public IActionResult LoginUser([FromForm] User user)
        {

            if (user.Email == null && user.Password == null)
            {
                return BadRequest();
            }

            CustomRequestPost req = new CustomRequestPost("api/home/LoginUser", user);
            var response = _requestService.SendPost(req, HttpContext);
            if (response == null)
            {
                return BadRequest("Неудачная попытка входа");
            }


            HttpContext.Response.Cookies.Append(".AspNetCore.Application.Id", response.result.ToString(),
              new CookieOptions
              {
                  MaxAge = TimeSpan.FromMinutes(60)
              });
            ViewData["Auth"] = true;

            return Redirect("/login");
        }


    }
}
