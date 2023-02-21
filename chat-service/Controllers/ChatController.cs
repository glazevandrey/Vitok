using Microsoft.AspNetCore.Mvc;

namespace chat_service.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
