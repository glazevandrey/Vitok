﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using web_app.Models.Requests.Get;
using web_app.Models.Requests;
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
            if(result.success == false)
            {
                return Redirect("/login");
            }
            ViewData["userid"] =Newtonsoft.Json.JsonConvert.DeserializeObject<web_server.Models.User>(result.result.ToString()).UserId;
            return View();
        }
    }
}
