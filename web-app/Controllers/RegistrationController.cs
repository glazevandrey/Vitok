using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using web_server.Models;
using web_server.Services.Interfaces;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/registration")]
    public class RegistrationController : Controller
    {
        IJsonService _jsonService;
        IRequestService _requestService;
        public RegistrationController(IJsonService jsonService, IRequestService requestService)
        {
            _jsonService = jsonService;
            _requestService = requestService;
        }
        [HttpGet]
        public IActionResult Index([FromQuery] string id)
        {
            if (id != null)
            {
                ViewData["Id"] = id;
            }

            return View();
        }

        [HttpPost]
        public IActionResult Register([FromQuery] string id, [FromForm] User user)
        {
            if (id != null)
            {
                user.UserId = Convert.ToInt32(id);


            }
            else
            {
                user.UserId = web_server.DbContext.TestData.UserList.Last().UserId + 1;
            }
            user.Role = "Student";
            CustomRequestPost req = new CustomRequestPost("api/home/registeruser", user);
            var response = _requestService.SendPost(req, HttpContext);
            if (response == null)
            {
                return BadRequest("Неудачная попытка входа");
            }
            if (id != null)
            {

                CustomRequestGet req2 = new GetRegistrationByUserId(user.UserId.ToString());
                var response2 = _requestService.SendGet(req2, HttpContext);
                if (response.success == false)
                {
                    return BadRequest("Что-то пошло не так =(");
                }

                var sch = Newtonsoft.Json.JsonConvert.DeserializeObject<Registration>(response2.result.ToString());
                if (sch != null)
                {
                    foreach (var item in sch.WantThis.dateTimes)
                    {
                        var req3 = new CustomRequestPost("api/tutor/removetutortime", $"{sch.TutorId};{item}");
                        var response3 = _requestService.SendPost(req3, HttpContext);
                        if (response3 == null)
                        {
                            return BadRequest("Неудачная попытка входа");
                        }
                    }
                }
            }

            var hub = new HubConnectionBuilder().WithUrl(Program.web_server_ip + "/chatHub").Build();
            hub.StartAsync();

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
