using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using web_app.Requests;
using web_app.Requests.Get;
using web_app.Services;
using web_server.Models.DBModels;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/contacts")]
    public class ContactsController : Controller
    {
        IRequestService _requestService;
        public ContactsController(IRequestService requestService)
        {
            _requestService = requestService;
        }
        public IActionResult Index()
        {

            CustomRequestGet req = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res = _requestService.SendGet(req, HttpContext);

            if (!res.success)
            {
                return Redirect("/login");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString(), Program.settings);
            var model = user.Schedules.Where(m => m.UserName != "" && m.UserName != null).ToList();

            ViewData["role"] = user.Role;

            if (user.Role == "Student")
            {
                ViewData["lessons"] = ((Student)user).LessonsCount;
            }
            ViewData["usertoken"] = user.UserId;
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;
            if (user.Role == "Student")
            {
                ViewData["firstLogin"] = ((Student)user).FirstLogin;
            }

            List<User> model2;
            CustomRequestGet req2;
            if (user.Role == "Student")
            {
                req2 = new GetAllStudentTutorsRequest(user.UserId.ToString());
            }
            else
            {
                req2 = new GetAllTutorStudentsRequest(user.UserId.ToString());
            }

            var res2 = _requestService.SendGet(req2, HttpContext);
            model2 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(res2.result.ToString(), Program.settings);

            ViewData["users"] = model2;


            return View(model);

        }


        [HttpPost("rejection", Name = "rejection")]
        public IActionResult Rejection([FromForm] string userIdReject, [FromForm] string tutorIdReject)
        {
            var req = new CustomRequestPost("api/tutor/rejectStudent", $"{tutorIdReject};{userIdReject}");
            _requestService.SendPost(req, HttpContext);

            return RedirectToAction("Index", "Contacts");
        }
    }
}
