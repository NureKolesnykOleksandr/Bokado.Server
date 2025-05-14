using Microsoft.AspNetCore.Mvc;

namespace Bokado.Server.Controllers
{
    public class ChallengeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
