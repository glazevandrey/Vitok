using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using web_app.Models;
using web_app.Models.Requests;

namespace web_app.Services
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
