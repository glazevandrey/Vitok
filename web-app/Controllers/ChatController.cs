using Microsoft.AspNetCore.Mvc;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using web_server.Services;

namespace web_app.Controllers
{
    public class ChatController : Controller
    {
        IJsonService _jsonService;
        IRequestService _requestService;
        public ChatController(IJsonService jsonService, IRequestService requestService)
        {
            _jsonService = jsonService;
            _requestService = requestService;
        }
        public IActionResult Index()
        {
            CustomRequestGet request = new GetUserByToken(Request.Cookies[".AspNetCore.Application.Id"]);
            var result = _requestService.SendGet(request);
            if (result.success == false)
            {
                return Redirect("/login");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<web_server.Models.User>(result.result.ToString());

            ViewData["userid"] = user.UserId;
            ViewData["role"] = user.Role;
            ViewData["usertoken"] = user.UserId;

            return View();
        }
    }
}
