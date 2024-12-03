using Microsoft.AspNetCore.Mvc;

namespace MH_TicketingSystem.Controllers
{
    public class AccessDeniedController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
