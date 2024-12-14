using MH_TicketingSystem.Attributes;
using MH_TicketingSystem.Hubs;
using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MH_TicketingSystem.Controllers
{

    /// <summary>
    /// This controller manage the dashboard UI - Getting the updated 
    /// number of tickets depends on the status 
    /// Get all tickets
    /// </summary>
    public class DashboardController : Controller
    {

        public readonly ApplicationDbContext _context;
        private readonly IHubContext<DashboardHub> _hubContext;

        public DashboardController(ApplicationDbContext context, IHubContext<DashboardHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Load the dashboard page ang get the stat of the tickets
        /// </summary>
        /// <returns></returns>
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


        /// <summary>
        /// Get the today's tickets -- use also in dashboard page
        /// </summary>
        /// <returns></returns>
        public async Task<List<TicketViewModel>> GetTodayTickets()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Get all today's ticket
            var tickets = await (from t in _context.Tickets
                                 join pl in _context.PriorityLevels on t.PriorityLevelId equals pl.Id
                                 orderby t.TicketStatus ascending, t.DateTicket descending
                                 where (t.DateTicket >= today && t.DateTicket < tomorrow)
                                 select new TicketViewModel
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


        [HttpGet]
        public IActionResult GetActiveTicketsPerMonth()
        {
            // Get data grouped by month
            var data = Enumerable.Range(1, 12).Select(month => new
            {
                Month = month,
                Count = _context.Tickets
                    .Where(t => t.DateTicket.Month == month && t.DateTicket.Year == DateTime.Now.Year)
                    .Count()
            }).ToList();

            var chartData = new
            {
                labels = data.Select(d => new DateTime(1, d.Month, 1).ToString("MMM")), // Month names
                datasets = new[]
                {
                new
                {
                    label = "Active Tickets",
                    borderColor = "#1d7af3",
                    pointBorderColor = "#FFF",
                    pointBackgroundColor = "#1d7af3",
                    pointBorderWidth = 2,
                    pointHoverRadius = 4,
                    pointHoverBorderWidth = 1,
                    pointRadius = 4,
                    backgroundColor = "transparent",
                    fill = true,
                    borderWidth = 2,
                    data = data.Select(d => d.Count) // Count for each month
                }
            }
            };

            return Json(chartData);
        }


        [HttpGet]
        public IActionResult GetTicketsByPriority()
        {
            // Define Bootstrap color mappings
            var bootstrapColors = new Dictionary<string, string>
            {
                { "primary", "#007bff" },
                { "secondary", "#6c757d" },
                { "info", "#17a2b8" },
                { "success", "#28a745" },
                { "danger", "#dc3545" },
                { "warning", "#ffc107" }
            };

            var data = _context.Tickets
                .GroupBy(t => new { t.PriorityLevel.PriorityLevelName, t.PriorityLevel.PriorityLevelColor })
                .Select(g => new
                {
                    Priority = g.Key.PriorityLevelName,
                    Count = g.Count(),
                    Color = bootstrapColors.ContainsKey(g.Key.PriorityLevelColor)
                        ? bootstrapColors[g.Key.PriorityLevelColor]
                        : "#000000" // Default to black if no mapping found
                })
                .ToList();

            var chartData = new
            {
                labels = data.Select(d => d.Priority).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        data = data.Select(d => d.Count).ToArray(),
                        backgroundColor = data.Select(d => d.Color).ToArray()
                    }
                }
            };

            return Json(chartData);
        }


    }
}
