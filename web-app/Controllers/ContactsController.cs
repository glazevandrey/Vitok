using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
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

            CustomRequestGet request = new GetSchedulesByUserToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var result = _requestService.SendGet(request, HttpContext);
            var model = new List<Schedule>();
            model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(result.result.ToString());
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
            if (user.Role == "Student")
            {
                ViewData["firstLogin"] = ((Student)user).FirstLogin;
            }


            //var model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(result.result.ToString());
            //model.RemoveAll(m => m.UserId == 1);
            List<User> model2;
            CustomRequestGet req2;
            if(user.Role == "Student")
            {
                req2 = new GetAllStudentTutorsRequest(user.UserId.ToString());
            }
            else
            {
                req2 = new GetAllTutorStudentsRequest(user.UserId.ToString());
            }

            var res2 = _requestService.SendGet(req2, HttpContext);
            model2 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(res2.result.ToString(), Program.settings);

            //foreach (var item in model)
            //{
            //    CustomRequestGet req2;

            //    if (user.Role == "Student")
            //    {
            //        if (model2.FirstOrDefault(m => m.UserId == item.TutorId) == null)
            //        {
            //            req2 = new GetUserById(item.TutorId.ToString() + $";{user.Role}");
            //            var res2 = _requestService.SendGet(req2, HttpContext);
            //            model2.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<Tutor>(res2.result.ToString(), Program.settings));
            //        }

            //    }
            //    else
            //    {
            //        if (model2.FirstOrDefault(m => m.UserId == item.UserId) == null)
            //        { 
            //            req2 = new GetUserById(item.UserId.ToString() + $";{user.Role}");
            //            var res2 = _requestService.SendGet(req2, HttpContext);
            //            model2.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<Student>(res2.result.ToString(), Program.settings));
            //        }

            //    }
            //}
            ViewData["users"] = model2;


                return View(model);
            
            //return View();
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
