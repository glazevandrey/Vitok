using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using web_application.Models;
using web_application.Models.Requests;

namespace web_application.Services
{
    public interface IRequestService
    {
        public ResponseModel SendGet(CustomRequestGet request);
        public IActionResult JsonToResult(string json);
        public ResponseModel SendGet(CustomRequestGet request, HttpContext context);
        public ResponseModel SendPost(CustomRequestPost req);
        public ResponseModel SendPost(CustomRequestPost req, HttpContext context);
    }
}
