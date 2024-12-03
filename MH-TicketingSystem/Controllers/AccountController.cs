using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using MH_TicketingSystem.Services;
using MH_TicketingSystem.Models;


namespace MH_TicketingSystem.Controllers
{
    public class AccountController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

    }
}
