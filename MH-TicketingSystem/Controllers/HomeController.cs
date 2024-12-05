using MH_TicketingSystem.Migrations;
using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Versioning;
using System.Collections.Generic;


namespace MH_TicketingSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ApplicationDbContext context, 
                            IWebHostEnvironment webHostEnvironment, 
                            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string ticketType = "all", string messageAlert = "", 
                    int errorCount = 0)
        {

            List<TicketPriorityLevelViewModel> tickets = null; // stored tickets
                                                               // Get UserID the one that is login
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return View();

            // Assuming GetAllTickets() returns an IEnumerable<Ticket>
            var allTickets = await GetAllTickets();

            if (User.IsInRole("Admin"))
            {
                // Filter tickets based on status only (if admin)
                tickets = ticketType switch
                {
                    "open" => allTickets.Where(t => t.TicketStatus == (int)TicketStatus.Open).ToList(),
                    "closed" => allTickets.Where(t => t.TicketStatus == (int)TicketStatus.Closed).ToList(),
                    _ => allTickets.ToList() // Return all tickets for any other type
                };
            }
            else
            {
                // Filter tickets based on user and status (if not admin)
                tickets = ticketType switch
                {
                    "open" => allTickets.Where(t => t.TicketUserId == user.Id && t.TicketStatus == (int)TicketStatus.Open).ToList(),
                    "closed" => allTickets.Where(t => t.TicketUserId == user.Id && t.TicketStatus == (int)TicketStatus.Closed).ToList(),
                    _ => allTickets.Where(t => t.TicketUserId == user.Id).ToList() // Return user's tickets only
                };
            }

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

        public async Task<List<TicketPriorityLevelViewModel>> GetAllTickets()
        {
            var tickets = await (from t in _context.Tickets
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
                            }).ToListAsync();
            return tickets;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.PriorityLevel = GetPriorityLevels();

            // Get UserID the one that is login
            var user = await _userManager.GetUserAsync(User);
            int ticketCount = (from t in _context.Tickets
                                    where t.UserId == user.Id
                                    select t.UserId).Count();

            ticketCount++;
            ViewBag.TicketNumber = ticketCount;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Tickets ticket, IFormFile file = null)
        {
            ViewBag.PriorityLevel = GetPriorityLevels();
            if (ModelState.IsValid)
            {
                string fileName = "";
                string fileSavePath = "";
                if (file != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/tickets");

                    // If the folder did not exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        // create
                        Directory.CreateDirectory(uploadsFolder);
                    }

					fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
					fileSavePath = Path.Combine(uploadsFolder, fileName);

                    // Upload File
                    using (FileStream stream =  new FileStream(fileSavePath, FileMode.Create))
                    {
                      await file.CopyToAsync(stream);
                    }

                    // Save the file Info to Ticket Model
                    ticket.FilePath = fileSavePath;
                    ticket.FileName = fileName;
                }

                // Get UserID the one that is login
                var user = await _userManager.GetUserAsync(User);
                ticket.UserId = user.Id;

                // Ticket Status
                ticket.TicketStatus = (int)TicketStatus.Open;
                
                // Date Ticket
                ticket.DateTicket = DateTime.Now;

                try
                {
                    await _context.Tickets.AddAsync(ticket);
                    await _context.SaveChangesAsync();
                    ViewBag.MessageAlert = "New ticket has been created.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.MessageAlert = $"Error adding new ticket: {ex.Message}";
                }
            }

			return View();
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
		public async Task UpdateTicketOpenBy(int id)
		{
			var ticket = await _context.Tickets.FindAsync(id);
			var user = await _userManager.GetUserAsync(User); // Get UserID of the logged-in user

            if (ticket != null && user != null && ticket.OpenBy == null)
			{
				ticket.OpenBy = user.Id;
				ticket.DateOpen = DateTime.Now;

				_context.Tickets.Update(ticket);
				await _context.SaveChangesAsync(); 
			}
		}

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


        public async Task<IActionResult> Search(string? search)
        {
            List<TicketPriorityLevelViewModel> searchTickets;

            if (string.IsNullOrEmpty(search))
            {
                searchTickets = await GetAllTickets();
            }
            else
            {
                var searchQuery = search.ToLower();

                searchTickets = await _context.Tickets
                    .Include(t => t.PriorityLevel) // Include the PriorityLevel relationship
                    .Where(t => EF.Functions.Like(t.TicketNumber.ToString(), $"%{searchQuery}%")
                                || EF.Functions.Like(t.Subject, $"%{searchQuery}%")
                                || EF.Functions.Like(t.Description, $"%{searchQuery}%"))
                    .Select(t => new TicketPriorityLevelViewModel
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
                        PriorityLevelId = t.PriorityLevel.Id,
                        PriorityLevelName = t.PriorityLevel.PriorityLevelName,
                        PriorityLevelColor = t.PriorityLevel.PriorityLevelColor
                    })
                    .ToListAsync();
            }
            return View(searchTickets);
        }


        [NonAction]
        public List<SelectListItem> GetPriorityLevels()
        {
            var priorityLevel = _context.PriorityLevels
                     .Select(pL => new SelectListItem
                     {
                         Text = pL.PriorityLevelName,
                         Value = pL.Id.ToString()
                     }).ToList();

            return priorityLevel;
        }


		// NOTES : ERROR
		/**
         * async void is not awaited: The method UpdateTicketOpenBy is declared as async void, 
         * which makes it a "fire-and-forget" operation. This means the method's execution is 
         * not awaited or tracked, leading to potential issues such as DbContext disposal before completion.
         */
	}

}
