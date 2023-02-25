using Microsoft.AspNetCore.Mvc;

namespace web_app.Controllers
{
    public class ContactsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
