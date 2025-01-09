using MH_TicketingSystem.Hubs;
using MH_TicketingSystem.Migrations;
using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Versioning;
using System.Collections.Generic;
using System.Net.Sockets;


namespace MH_TicketingSystem.Controllers
{

    /// <summary>
    /// This controller use in managing of creating, closing, reopening the ticket by the use
    /// Also this controller use real-time updating the dashboard after changing status of the ticket
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHubContext<DashboardHub> _hubContext;

        public HomeController(ApplicationDbContext context,
                            IWebHostEnvironment webHostEnvironment,
                            UserManager<IdentityUser> userManager,
                            IHubContext<DashboardHub> hubContext)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Use in loading the home page by the user, it gets the 
        /// ticket base on the ticketType click by the user, but first loading the
        /// home page it get all tickets
        /// 
        /// It also use in redirecting the page after the user close the ticket or reopen
        /// </summary>
        /// <param name="ticketType"></param>
        /// <param name="messageAlert"></param>
        /// <param name="errorCount"></param>
        /// <returns></returns>

        public async Task<IActionResult> Index(string ticketType = "today", string messageAlert = "", int errorCount = 0)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return View();

            var isAdmin = User.IsInRole("Admin");
            var tickets = await GetFilteredTickets(ticketType, user.Id, isAdmin);

            if (!string.IsNullOrEmpty(messageAlert))
            {
                TempData[errorCount == 0 ? "SuccessMessage" : "ErrorMessage"] = messageAlert;
            }

            return View(tickets);
        }


        /// <summary>
        /// Get Filtered Tickets 
        /// </summary>
        /// <returns> all tickets </returns>
        public async Task<List<TicketViewModel>> GetFilteredTickets(string ticketType, string userId, bool isAdmin)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            IQueryable<Tickets> query = _context.Tickets
                .Include(t => t.PriorityLevel)
                .Include(t => t.User)
                .Include(t => t.ClosedByUser)
                .Include(t => t.OpenedByUser)
                .Include(t => t.TicketConversations);


            // Filter by role
            if (!isAdmin)
            {
                query = query.Where(t => t.UserId == userId);
            }

            // Apply ticketType filter
            query = ticketType switch
            {
                "today" => query.Where(t => t.DateTicket >= today && t.DateTicket < tomorrow),
                "open" => query.Where(t => t.TicketStatus == (int)TicketStatus.Open),
                "pending" => query.Where(t => t.TicketStatus == (int)TicketStatus.Pending),
                "closed" => query.Where(t => t.TicketStatus == (int)TicketStatus.Closed),
                _ => query // No filter for other types
            };

            return await query
                .OrderBy(t => t.TicketStatus)
                .ThenByDescending(t => t.DateTicket)
                .Select(t => new TicketViewModel
                {
                    TicketUserId = t.UserId,
                    TicketId = t.Id,
                    TicketNumber = t.TicketNumber,
                    Subject = t.Subject,
                    Description = t.Description,
                    OpenBy = t.OpenedByUser != null ? t.OpenedByUser.UserName : null,
                    OpenDateTime = t.DateOpen,
                    ClosedBy = t.ClosedByUser != null ? t.ClosedByUser.UserName : null,
                    ClosedDateTime = t.DateClose,
                    FilePath = t.FilePath,
                    FileName = t.FileName,
                    DateTicket = t.DateTicket,
                    TicketStatus = t.TicketStatus,
                    TicketStatusString = t.TicketStatus == (int)TicketStatus.Open ? "Open" :
                                          t.TicketStatus == (int)TicketStatus.Closed ? "Closed" :
                                          t.TicketStatus == (int)TicketStatus.Pending ? "Pending" :
                                          "Unknown",
                    SLADeadline = t.SLADeadline,
                    Resolution = t.Resolution,
                    PriorityLevelId = t.PriorityLevel.Id,
                    PriorityLevelName = t.PriorityLevel.PriorityLevelName,
                    PriorityLevelColor = t.PriorityLevel.PriorityLevelColor,
                    TicketBy = t.User.UserName,
                    TicketReplies = t.TicketConversations.Count // Count replies
                }).ToListAsync();
        }


        /// <summary>
        ///  Use in Creating form UI in creating ticket
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.PriorityLevel = GetPriorityLevels();
            ViewBag.TicketNumber = GetTicketNumber();
            return View();
        }

        public int GetTicketNumber()
        {
             // Get UserID the one that is login
            var user = _userManager.GetUserId(User);
            int ticketCount = (from t in _context.Tickets
                               where t.UserId == user
                               select t.UserId).Count();
            ticketCount += 1;
            return ticketCount;
        }



        /// <summary>
        /// Use in posting the ticket / ticket creation
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create(Tickets ticket, IFormFile file = null)
        {
            ViewBag.PriorityLevel = GetPriorityLevels();
            ViewBag.TicketNumber = GetTicketNumber();
            string messageAlert = "";
            int errorCount = 0;
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

                    var holdFileNameTemp = Path.GetFileName(file.FileName);
                    fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    fileSavePath = Path.Combine(uploadsFolder, fileName);

                    // Upload File
                    using (FileStream stream = new FileStream(fileSavePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Save the file Info to Ticket Model
                    ticket.FilePath = fileSavePath;
                    ticket.FileName = holdFileNameTemp;
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
                    messageAlert = "New ticket has been created.";

                    // Dashboard Realtime Update
                    int totalTickets = _context.Tickets.Count();
                    int newTickets = _context.Tickets.Count(t => t.DateTicket == DateTime.Today);
                    int openTickets = _context.Tickets.Count(t => t.TicketStatus == (int)TicketStatus.Open);

                    // Notify the dashboard
                    await _hubContext.Clients.All.SendAsync("UpdateMetric", "totalTickets", totalTickets);
                    await _hubContext.Clients.All.SendAsync("UpdateMetric", "newTickets", newTickets);
                    await _hubContext.Clients.All.SendAsync("UpdateMetric", "openTickets", openTickets);

                    // Get Other info in displaying ticket real-time
                    var ticketStatus = ticket.TicketStatus == (int)TicketStatus.Open ? "Open" :
                                              ticket.TicketStatus == (int)TicketStatus.Closed ? "Closed" :
                                              ticket.TicketStatus == (int)TicketStatus.Pending ? "Pending" :
                                              "Unknown";

                    var priorityLevelColor = _context.PriorityLevels
                            .FirstOrDefault(c => c.Id == ticket.PriorityLevelId)?.PriorityLevelColor;

                    await _hubContext.Clients.All.SendAsync("ReceiveNewTicket", ticket, ticketStatus, priorityLevelColor);
                    return RedirectToAction("Index", new { ticketType = "today", messageAlert, errorCount });
                }
                catch (Exception ex)
                {
                    ViewBag.MessageAlert = $"Error adding new ticket: {ex.Message}";
                }
            }

            return View();
        }

        /// <summary>
        /// Getting the details of specific ticket
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int id)
        {
            // Get the details of the ticket
            TicketViewModel ticket = await GetTicketDetails(id);

            // Get the department -- this is use in UI
            if (User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value == "Admin")
            {
                // Get the Department Name the one who made the ticket
                TempData["DepartmentName"] = await (from t in _context.Tickets
                                            join u in _context.Users on t.UserId equals u.Id
                                            join ud in _context.UserDepartments on u.Id equals ud.UserId
                                            join d in _context.Departments on ud.DepartmentId equals d.Id
                                            select d.DepartmentName
                                            ).FirstOrDefaultAsync();
            }
            else
            {
                // The one who made the ticket will only show the 
                // Department who is admin of the system
                TempData["DepartmentName"] = await (from r in _context.Roles
                                          join d in _context.Departments on r.Id equals d.RoleId
                                          where r.Name == "Admin"
                                          select d.DepartmentName
                                          ).FirstOrDefaultAsync();
            }


            // Only the admin or the I.T
            if (User.IsInRole("Admin") && ticket.OpenBy == null)
            {
                // If the admin/I.T open the ticket 
                // update the database
                await UpdateTicketOpenBy(id);
            }

            // Get the conversation ticket
            var userTicketConvo = await (from tc in _context.TicketConversation
                                         join u in _context.Users on tc.UserID equals u.Id
                                         join ud in _context.UserDepartments on u.Id equals ud.UserId
                                         join d in _context.Departments on ud.DepartmentId equals d.Id
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

            TicketDetailsViewModel ticketDetails = new TicketDetailsViewModel
            {
                Ticket = ticket,
                TicketConversation = userTicketConvo,
            };

            return View(ticketDetails);
        }

        private async Task<TicketViewModel> GetTicketDetails(int id)
        {
            var ticket = await (from t in _context.Tickets
                                join pl in _context.PriorityLevels on t.PriorityLevelId equals pl.Id
                                join u in _context.Users on t.UserId equals u.Id into userJoin
                                from ticketUser in userJoin.DefaultIfEmpty()
                                join openUser in _context.Users on t.OpenBy equals openUser.Id into openUserJoin
                                from openUser in openUserJoin.DefaultIfEmpty()
                                join closeUser in _context.Users on t.CloseBy equals closeUser.Id into closeUserJoin
                                from closeUser in closeUserJoin.DefaultIfEmpty()
                                where t.Id == id
                                select new TicketViewModel
                                {
                                    TicketUserId = t.UserId,
                                    TicketId = t.Id,
                                    TicketNumber = t.TicketNumber,
                                    Subject = t.Subject,
                                    Description = t.Description,
                                    OpenBy = openUser != null ? openUser.UserName : null,
                                    OpenDateTime = t.DateOpen,
                                    ClosedBy = closeUser != null ? closeUser.UserName : null,
                                    ClosedDateTime = t.DateClose,
                                    FilePath = t.FilePath,
                                    FileName = t.FileName,
                                    DateTicket = t.DateTicket,
                                    TicketStatus = t.TicketStatus,
                                    SLADeadline = t.SLADeadline,
                                    Resolution = t.Resolution,
                                    PriorityLevelId = pl.Id,
                                    PriorityLevelName = pl.PriorityLevelName,
                                    PriorityLevelColor = pl.PriorityLevelColor,
                                    TicketBy = ticketUser != null ? ticketUser.UserName : null
                                }).FirstOrDefaultAsync();


            return ticket;
        }


        /// <summary>
        /// Update ticket - User who open and the date opened
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Close the ticket -- update the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

                    // Update Dashboard RealTime
                    int openTickets = _context.Tickets.Count(t => t.TicketStatus == (int)TicketStatus.Open);
                    int closedTickets = _context.Tickets.Count(t => t.TicketStatus == (int)TicketStatus.Closed);
                    await _hubContext.Clients.All.SendAsync("UpdateMetric", "openTickets", openTickets);
                    await _hubContext.Clients.All.SendAsync("UpdateMetric", "closedTickets", closedTickets);

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

            return RedirectToAction("Index", new { ticketType = "today", messageAlert, errorCount });
        }


        /// <summary>
        /// Make the ticket pending -- only admin role can make the ticket pending
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

            return RedirectToAction("Index", new { ticketType = "today", messageAlert, errorCount });
        }


        /// <summary>
        /// Re-open the ticket
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> ReOpenTicket(int id)
        {
            Tickets ticket = await _context.Tickets.FindAsync(id);
            string messageAlert = "";
            int errorCount = 0;
            if (ticket != null && ticket.TicketStatus == 2)
            {
                ticket.TicketStatus = (int)TicketStatus.Open;
                ticket.CloseBy = null;
                ticket.DateClose = null;
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                    // Update Dashboard RealTime
                    int openTickets = _context.Tickets.Count(t => t.TicketStatus == (int)TicketStatus.Open);
                    int closedTickets = _context.Tickets.Count(t => t.TicketStatus == (int)TicketStatus.Closed);
                    await _hubContext.Clients.All.SendAsync("UpdateMetric", "openTickets", openTickets);
                    await _hubContext.Clients.All.SendAsync("UpdateMetric", "closedTickets", closedTickets);
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

            return RedirectToAction("Index", new { ticketType = "today", messageAlert, errorCount });
        }

        /// <summary>
        ///  Use in searching ticket
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<IActionResult> Search(string? search)
        {
            List<TicketViewModel> searchTickets;

            if (string.IsNullOrEmpty(search))
            {
                searchTickets = new List<TicketViewModel>();
            }
            else
            {
                var searchQuery = search.ToLower();
                var user = await _userManager.GetUserAsync(User);
                searchTickets = await GetSearchTicket(searchQuery);
                if (!User.IsInRole("Admin"))
                {
                    searchTickets = searchTickets.Where(t => t.TicketUserId == user.Id).ToList();
                }
            }
            return View(searchTickets);
        }

        /// <summary>
        /// Get all tickets base on search query
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        private async Task<List<TicketViewModel>> GetSearchTicket(string searchQuery)
        {
            List<TicketViewModel> results = await _context.Tickets
                    .Include(t => t.PriorityLevel)
                    .Include(t => t.User)
                    .Where(t => EF.Functions.Like(t.TicketNumber.ToString(), $"%{searchQuery}%")
                                || EF.Functions.Like(t.Subject, $"%{searchQuery}%")
                                || EF.Functions.Like(t.Description, $"%{searchQuery}%")
                                || EF.Functions.Like(t.User.UserName, $"%{searchQuery}%"))
                    .Select(t => new TicketViewModel
                    {
                        TicketUserId = t.UserId,
                        TicketId = t.Id,
                        TicketNumber = t.TicketNumber,
                        Subject = t.Subject,
                        Description = t.Description,
                        TicketBy = t.User.UserName,
                        FilePath = t.FilePath ?? null,
                        FileName = t.FileName ?? null,
                        DateTicket = t.DateTicket,
                        TicketStatus = t.TicketStatus,
                        TicketStatusString = t.TicketStatus == (int)TicketStatus.Open ? "Open" :
                                              t.TicketStatus == (int)TicketStatus.Closed ? "Closed" :
                                              t.TicketStatus == (int)TicketStatus.Pending ? "Pending" :
                                              "Unknown",
                        SLADeadline = (DateTime)t.SLADeadline,
                        PriorityLevelId = t.PriorityLevel.Id,
                        PriorityLevelName = t.PriorityLevel.PriorityLevelName,
                        PriorityLevelColor = t.PriorityLevel.PriorityLevelColor
                    })
                    .ToListAsync();

            return results;
        }



        /// <summary>
        /// Use by the get Create function to load the priority level in select element in chtml
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public List<SelectListItem> GetPriorityLevels()
        {
            var priorityLevel = _context.PriorityLevels
                     .Where(pl => pl.IsPriorityLevelActive == true)
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
