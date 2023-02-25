using Microsoft.AspNetCore.Mvc;
using web_server.DbContext;
using web_server.Models;
using web_server.Services;
using System;
using System.Linq;

namespace web_server.Controllers
{

    [ApiController]
    [Route("/api/[controller]")]
    public class HomeController : Controller
    {
        IAuthService _authService;
        IJsonService _jsonService;
        public HomeController(IAuthService authService, IJsonService jsonService)
        {
            _authService = authService;
            _jsonService = jsonService;
        }

        [HttpPost("loginuser", Name = "loginuser")]
        public string LoginUser()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(form.First().Key);
            var json = _authService.LogIn(user, HttpContext);
            return json;
        }

        [HttpPost("registeruser", Name = "registeruser")]
        public string RegisterUser()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(form.First().Key);
            var json = _authService.Register(user, HttpContext);
            return json;
        }
        [Authorize]
        [HttpGet("getuser", Name = "getuser")]
        public string GetUser([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var json = _authService.GetUserByToken(args);
            var check = _authService.CheckIsActive(HttpContext);
            if (check == false)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");

            }

            return json;
        }

        [HttpPost("adduserregistration", Name = "adduserregistration")]
        public string AddUserRegistration()
        {
            var form = Request.Form;
            if (form.Keys.Count != 0)
            {
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Registration>(form.First().Key);
                TestData.Registations.Add(model);
                var json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(model));
                return json;
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }
        [HttpGet("getregistration", Name = "getregistration")]
        public string GetRegistration([FromQuery] string args)
        {
            var json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(TestData.Registations.FirstOrDefault(m => m.UserId == Convert.ToInt32(args))));
            return json;
        }
        [HttpPost("getcontacts", Name = "GetContacts")]
        public string GetContacts()
        {
            var json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(TestData.Contacts));
            return json;
        }
    }
}
