using Microsoft.AspNetCore.Mvc;

namespace MH_TicketingSystem.Controllers
{
    public class ReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
