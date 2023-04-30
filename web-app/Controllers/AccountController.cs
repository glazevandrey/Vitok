using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using web_app.Requests;
using web_app.Requests.Get;
using web_app.Services;
using web_server.Models.DBModels;

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
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(result.result.ToString(), Program.settings);
            ViewData["usertoken"] = user.UserId;
            ViewData["userid"] = user.UserId;


            if (user.Role == "Student")
            {
                ViewData["lessons"] = ((Student)user).LessonsCount;
            }
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;

            if (error != null)
            {
                ViewData["error"] = error;
            }
            return View(user);
        }

        [HttpPost("saveinfo", Name = "saveinfo")]
        public IActionResult SaveInfo([FromForm] int userId,
    [FromForm] string firstName,
    [FromForm] string lastName,
    [FromForm] string photoUrl,
    [FromForm] DateTime birthDate,
    [FromForm] string email,
    [FromForm] string password,
    [FromForm] string phone,
    [FromForm] string wish, [FromForm] string role, [FromForm] string about
)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return RedirectToAction("Index", "Account", new { error = "Неудачная попытка сохранить данные" });
            }

            var student = new Student();
            var tutor = new Tutor();
            CustomRequestPost req = null;

            if (role == "Student")
            {
                student.FirstName = firstName.Trim();
                student.LastName = lastName.Trim();
                student.Role = role;
                student.About = about;
                student.Password = password;
                student.Phone = phone;
                student.Wish = wish;
                student.Email = email;
                student.PhotoUrl = photoUrl;
                student.BirthDate = birthDate;
                student.UserId = userId;
                req = new CustomRequestPost("api/account/saveuserinfo", student);
            }
            else
            {
                tutor.FirstName = firstName.Trim();
                tutor.LastName = lastName.Trim();
                tutor.Role = role;
                tutor.About = about;
                tutor.Password = password;
                tutor.Phone = phone;
                tutor.Wish = wish;
                tutor.Email = email;
                tutor.PhotoUrl = photoUrl;
                tutor.BirthDate = birthDate;
                tutor.UserId = userId;
                req = new CustomRequestPost("api/account/saveuserinfo", tutor);

            }


            var response = _requestService.SendPost(req, HttpContext);
            if (response == null)
            {
                return RedirectToAction("Index", "Account", new { error = "Неудачная попытка сохранить данные" });
            }

            return Redirect("/account");
        }


    }
}
