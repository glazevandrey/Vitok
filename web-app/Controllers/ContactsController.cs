using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using web_app.Requests;
using web_app.Requests.Get;
using web_app.Services;
using web_server.Models;
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

            CustomRequestGet request = new GetSchedulesByUserToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var result = _requestService.SendGet(request, HttpContext);

            CustomRequestGet req = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res = _requestService.SendGet(req, HttpContext);

            if (!res.success)
            {
                return Redirect("/login");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString(), Program.settings);
            ViewData["role"] = user.Role;

            if (user.Role == "Student")
            {
                ViewData["lessons"] = ((Student)user).LessonsCount;
            }
            ViewData["usertoken"] = user.UserId;
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;
            if (user.FirstLogin == true && user.Role == "Student")
            {
                ViewData["firstLogin"] = true;
            }
            if (result.success)
            {
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(result.result.ToString());
                model.RemoveAll(m => m.UserId == -1);
                var model2 = new List<User>();

                foreach (var item in model)
                {
                    CustomRequestGet req2;
                    if (user.Role == "Student")
                    {
                        req2 = new GetUserById(item.TutorId.ToString() + $";{user.Role}");
                        var res2 = _requestService.SendGet(req2, HttpContext);
                        model2.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<Tutor>(res2.result.ToString(), Program.settings));
                    }
                    else
                    {
                        req2 = new GetUserById(item.UserId.ToString() + $";{user.Role}");
                        var res2 = _requestService.SendGet(req2, HttpContext);
                        model2.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<Student>(res2.result.ToString(), Program.settings));

                    }
                }
                ViewData["users"] = model2;


                return View(model);
            }
            return View();
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
