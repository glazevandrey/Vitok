using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using web_server.Models;

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
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString());
            ViewData["role"] = user.Role;
            ViewData["lessons"] = user.LessonsCount;
            ViewData["usertoken"] = user.UserId;
            ViewData["photoUrl"] = user.PhotoUrl;

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
                        model2.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res2.result.ToString()));
                    }
                    else
                    {
                        req2 = new GetUserById(item.UserId.ToString() + $";{user.Role}");
                        var res2 = _requestService.SendGet(req2, HttpContext);
                        model2.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res2.result.ToString()));

                    }
                }
                ViewData["users"] = model2;


                return View(model);
            }
            return View();
        }
    }
}
