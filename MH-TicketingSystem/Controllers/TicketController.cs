using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            var tickets = (from t in _context.Tickets
                            join pl in _context.PriorityLevels on t.PriorityLevelId equals pl.Id
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
                                SLADeadline = (DateTime)t.SLADeadline,
                                PriorityLevelId = pl.Id,
                                PriorityLevelName = pl.PriorityLevelName,
                                PriorityLevelColor = pl.PriorityLevelColor
                            }).ToList();

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

        public async Task<IActionResult> Details(int id)
        {
            ViewBag.Ticket = await _context.Tickets.FindAsync(id);

            var userTicketConvo = await (from tc in _context.TicketConversation
                                         join u in _context.Users on tc.UserID equals u.Id
                                         join ur in _context.UserRoles on u.Id equals ur.UserId
                                         join r in _context.Roles on ur.RoleId equals r.Id
                                         join d in _context.Departments on r.Id equals d.RoleId
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

        [HttpPost]
        public async Task<IActionResult> CloseTicket(int id)
        {
            Tickets ticket = await _context.Tickets.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);
            string messageAlert = "";
            int errorCount = 0;
            if (ticket != null && user != null && ticket.TicketStatus == 0)
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


        [HttpPost]
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
