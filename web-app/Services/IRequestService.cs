using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using web_app.Requests;

namespace web_app.Services
{
    public interface IRequestService
    {
        public ResponseModel SendGet(CustomRequestGet request);
        public ResponseModel SendGet(CustomRequestGet request, HttpContext context);
        public ResponseModel SendPost(CustomRequestPost req, HttpContext context);
    }
}
