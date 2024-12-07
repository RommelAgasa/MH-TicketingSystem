using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MH_TicketingSystem.Controllers
{
    public class TicketController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TicketController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index(string ticketType = "all", string messageAlert = "", int errorCount = 0)
        {
            List<TicketPriorityLevelViewModel> tickets = ticketType switch
            {
                "open" => GetAllTickets().Where(t => t.TicketStatus == (int)TicketStatus.Open).ToList(),
                "closed" => GetAllTickets().Where(t => t.TicketStatus == (int)TicketStatus.Closed).ToList(),
                _ => GetAllTickets()
            };

            // This is use in opening and closing the ticket alert message
            if (!string.IsNullOrEmpty(messageAlert) && errorCount == 0)
            {
                ViewBag.SuccessMessage = messageAlert;
            }
            else if (!string.IsNullOrEmpty(messageAlert) && errorCount > 0)
            {
                ViewBag.ErrorMessage = messageAlert;
            }

            return View(tickets);
        }


        private List<TicketPriorityLevelViewModel> GetAllTickets()
        {
            var tickets = (from t in _context.Tickets
                           join pl in _context.PriorityLevels on t.PriorityLevelId equals pl.Id
                           orderby t.DateTicket descending
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
                           }).ToList();
            return tickets;
        }

        public async Task<IActionResult> Details(int id)
        {
            // Get the details of the ticket
            ViewBag.Ticket = await _context.Tickets.FindAsync(id);

            // Get the department -- this is use in UI
            if (User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value == "Admin")
            {
                // Get the Department Name the one who made the ticket
                ViewBag.DepartmentName = await (from t in _context.Tickets
                                                join u in _context.Users on t.UserId equals u.Id
                                                join ur in _context.UserRoles on u.Id equals ur.UserId
                                                join r in _context.Roles on ur.RoleId equals r.Id
                                                join d in _context.Departments on r.Id equals d.RoleId
                                                select d.DepartmentName
                                            ).FirstOrDefaultAsync();
            }
            else
            {
                // The one who made the ticket will only show the 
                // Department who is admin of the system
                ViewBag.DepartmentName = await (from r in _context.Roles
                                                join d in _context.Departments on r.Id equals d.RoleId
                                                where r.Name == "Admin"
                                                select d.DepartmentName
                                          ).FirstOrDefaultAsync();
            }

            // Get the conversation ticket
            var userTicketConvo = await (from tc in _context.TicketConversation
                                         join u in _context.Users on tc.UserID equals u.Id
                                         join ur in _context.UserRoles on u.Id equals ur.UserId
                                         join r in _context.Roles on ur.RoleId equals r.Id
                                         join d in _context.Departments on r.Id equals d.RoleId
                                         where tc.TicketId == id
                                         orderby tc.Timestamp ascending
                                         select new UserTicketConversation
                                         {
                                             UserId = u.Id,
                                             Department = d.DepartmentName,
                                             Username = u.UserName,
                                             Message = tc.Message,
                                             Timestamp = tc.Timestamp,
                                             FileName = tc.FileName,
                                             FilePath = tc.FilePath
                                         }).ToListAsync();

            // Only the admin or the I.T
            if (User.IsInRole("Admin"))
            {
                await UpdateTicketOpenBy(id); // Await the method
            }

            return View(userTicketConvo);
        }

        // Update ticket - User who open and the date opened
        public async Task UpdateTicketOpenBy(int id) // Change async void to async Task
        {
            var ticket = await _context.Tickets.FindAsync(id); // Use FindAsync for async operation
            var user = await _userManager.GetUserAsync(User); // Get UserID of the logged-in user

            if (ticket != null && user != null && ticket.OpenBy == null)
            {
                ticket.OpenBy = user.Id;
                ticket.DateOpen = DateTime.Now;

                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync(); // Use SaveChangesAsync for async operation
            }
        }

        public async Task<IActionResult> CloseTicket(int id)
        {
            Tickets ticket = await _context.Tickets.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);
            string messageAlert = "";
            int errorCount = 0;
            if (ticket != null && user != null && (ticket.TicketStatus == 0 || ticket.TicketStatus == 2))
            {
                ticket.TicketStatus = (int)TicketStatus.Closed;
                ticket.CloseBy = user.Id;
                ticket.DateClose = DateTime.Now;

                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                    messageAlert = "Ticket has been closed.";
                }
                catch (Exception ex)
                {
                    errorCount++;
                    messageAlert = $"Error adding new ticket: {ex.Message}";
                }
            }
            else
            {
                messageAlert = "Ticket is already closed.";
            }

            return RedirectToAction("Index", new { ticketType = "all", messageAlert, errorCount });
        }


        public async Task<IActionResult> PendingTicket(int id)
        {
            Tickets ticket = await _context.Tickets.FindAsync(id);
            string messageAlert = "";
            int errorCount = 0;
            if (ticket != null && ticket.TicketStatus == 0)
            {
                ticket.TicketStatus = (int)TicketStatus.Pending;
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                    messageAlert = "Successfully ticket pending.";

                }
                catch (Exception ex)
                {
                    errorCount++;
                    messageAlert = $"Error reopening the ticket: {ex.Message}";
                }

            }

            if (ticket.TicketStatus == (int)TicketStatus.Pending)
            {
                messageAlert = "Ticket is already in pending status.";
            }

            return RedirectToAction("Index", new { ticketType = "all", messageAlert, errorCount });
        }

        public async Task<IActionResult> ReOpenTicket(int id)
        {
            Tickets ticket = await _context.Tickets.FindAsync(id);
            string messageAlert = "";
            int errorCount = 0;
            if (ticket != null && ticket.TicketStatus == 1)
            {
                ticket.TicketStatus = (int)TicketStatus.Open;
                ticket.CloseBy = null;
                ticket.DateClose = null;
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                    messageAlert = "Successfully ticket reopen.";

                }
                catch (Exception ex)
                {
                    errorCount++;
                    messageAlert = $"Error reopening the ticket: {ex.Message}";
                }

            }

            if (ticket.TicketStatus == (int)TicketStatus.Pending)
            {
                messageAlert = "You cannot reopen ticket that is in pending status.";
            }

            return RedirectToAction("Index", new { ticketType = "all", messageAlert, errorCount });
        }



        [HttpGet]
        public IActionResult DownloadFile(string filePath, string fileName)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File path or name is missing.");
            }

            try
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
