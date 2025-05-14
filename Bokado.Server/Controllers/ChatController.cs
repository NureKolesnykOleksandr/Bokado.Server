using Microsoft.AspNetCore.Mvc;

namespace Bokado.Server.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
