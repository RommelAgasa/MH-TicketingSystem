using MH_TicketingSystem.Attributes;
using MH_TicketingSystem.Hubs;
using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MH_TicketingSystem.Controllers
{
    //[AccessDeniedAuthorize("Admin")]
    public class DashboardController : Controller
    {

        public readonly ApplicationDbContext _context;
        private readonly IHubContext<DashboardHub> _hubContext;

        public DashboardController(ApplicationDbContext context, IHubContext<DashboardHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            // Count Ticket
            int totalTickets = _context.Tickets.Count();

            var today = DateTime.Today; // E.g., 2024-12-05 00:00:00
            var tomorrow = today.AddDays(1); // E.g., 2024-12-06 00:00:00

            int newTickets = _context.Tickets.Count(t => t.DateTicket >= today && t.DateTicket < tomorrow);
            int openTickets = _context.Tickets.Count(t => t.TicketStatus == (int)TicketStatus.Open);
            int closedTickets = _context.Tickets.Count(t => t.TicketStatus == (int)TicketStatus.Closed);

            ViewBag.TicketStat = new
            {
                TotalTickets = totalTickets,
                NewTickets = newTickets,
                OpenTickets = openTickets,
                ClosedTickets = closedTickets
            };

            var todayTickets = await GetTodayTickets();
            return View(todayTickets);
        }

        public async Task<List<TicketPriorityLevelViewModel>> GetTodayTickets()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var tickets = await (from t in _context.Tickets
                                 join pl in _context.PriorityLevels on t.PriorityLevelId equals pl.Id
                                 where t.DateTicket >= today && t.DateTicket < tomorrow
                                 select new TicketPriorityLevelViewModel
                                 {
                                     TicketUserId = t.UserId,
                                     TicketId = t.Id,
                                     TicketNumber = t.TicketNumber,
                                     Subject = t.Subject,
                                     Description = t.Description,
                                     FilePath = t.FilePath ?? null,
                                     FileName = t.FileName ?? null,
                                     DateTicket = t.DateTicket,
                                     TicketStatus = t.TicketStatus,
                                     TicketStatusString = t.TicketStatus == (int)TicketStatus.Open ? "Open" :
                                              t.TicketStatus == (int)TicketStatus.Closed ? "Closed" :
                                              t.TicketStatus == (int)TicketStatus.Pending ? "Pending" :
                                              "Unknown",
                                     SLADeadline = (DateTime)t.SLADeadline,
                                     PriorityLevelId = pl.Id,
                                     PriorityLevelName = pl.PriorityLevelName,
                                     PriorityLevelColor = pl.PriorityLevelColor
                                 }).ToListAsync();
            return tickets;
        }
    }
}
