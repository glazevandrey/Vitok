using Microsoft.AspNetCore.Mvc;
using web_app.Models.Requests.Get;
using web_app.Models.Requests;
using web_app.Services;
using web_server.Services;
using System.Collections.Generic;
using web_server.Models;
using System.Linq;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/contacts")]
    public class ContactsController : Controller
    {
        IJsonService _jsonService;
        IRequestService _requestService;
        public ContactsController(IJsonService jsonService, IRequestService requestService)
        {
            _jsonService = jsonService;
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
               return  Redirect("/login");
            }

            ViewData["role"] = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString()).Role;

            if (result.success)
            {
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(result.result.ToString());
                model.RemoveAll(m=>m.UserId == -1);
                var model2 = new List<User>();
                if (ViewData["role"].ToString() == "Student")
                {
                    foreach (var item in model)
                    {
                        CustomRequestGet req2 = new GetUserById(item.TutorId.ToString());
                        var res2 = _requestService.SendGet(req2, HttpContext);
                        model2.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res2.result.ToString()));
                    }
                    ViewData["users"] = model2;
                }
               
                return View(model);
            }
            return View();
        }
    }
}
