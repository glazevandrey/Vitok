using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using web_app.Requests;
using web_app.Services;
using web_server.Models.DBModels;
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
        public IActionResult Index([FromQuery] string id, [FromQuery] string error = null)
        {
            if (id != null)
            {
                ViewData["Id"] = id;
            }
            if(error != null)
            {
                ViewData["error"] = error;
            }

            return View();
        }

        [HttpPost]
        public IActionResult Register([FromQuery] string id, [FromForm] Student user)
        {
            if (id != null)
            {
                user.UserId = 0;

            }
            user.Role = "Student";
            CustomRequestPost req = new CustomRequestPost($"api/home/registeruser?id={id}", user);
            var response = _requestService.SendPost(req, HttpContext);
            if (response == null)
            {
                return RedirectToAction("Index", new { error = "Такой E-mail уже занят"});
            }

            if (id != null)
            {

                if (response.success == false)
                {
                    return BadRequest("Что-то пошло не так =(");
                }

            }
            ViewData["Auth"] = true;

            return Redirect("/login");
        }
    }
}
