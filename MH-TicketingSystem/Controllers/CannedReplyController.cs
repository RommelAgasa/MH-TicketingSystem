using Microsoft.AspNetCore.Mvc;

namespace MH_TicketingSystem.Controllers
{
    public class CannedReplyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
