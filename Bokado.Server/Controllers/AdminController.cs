using Microsoft.AspNetCore.Mvc;

namespace Bokado.Server.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
