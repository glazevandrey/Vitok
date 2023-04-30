using Microsoft.AspNetCore.Mvc;
using web_app.Requests;
using web_app.Requests.Get;
using web_app.Services;
using web_server.Models.DBModels;

namespace web_app.Controllers
{
    public class ChatController : Controller
    {
        IRequestService _requestService;
        public ChatController(IRequestService requestService)
        {
            _requestService = requestService;
        }
        public IActionResult Index()
        {
            CustomRequestGet request = new GetLiteUserByToken(Request.Cookies[".AspNetCore.Application.Id"]);
            var result = _requestService.SendGet(request, HttpContext);

            if (result.success == false)
            {
                return Redirect("/login");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(result.result.ToString(), Program.settings);
            if (user.Role == "Student")
            {
                var req5 = new CustomRequestPost("api/account/removeFirstLogin", user.UserId.ToString());
                _requestService.SendPost(req5, HttpContext);
            }
            ViewData["userid"] = user.UserId;
            ViewData["role"] = user.Role;

            if (user.Role == "Student")
            {
                ViewData["lessons"] = ((Student)user).LessonsCount;
            }
            ViewData["usertoken"] = user.UserId;
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;

            return View();
        }
    }
}
