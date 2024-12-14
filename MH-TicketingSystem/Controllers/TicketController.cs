using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MH_TicketingSystem.Controllers
{

    /// <summary>
    /// This controller use to manage the ticket status and updating the data
    /// </summary>
    public class TicketController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TicketController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Use in loading the ticket page in the dashboard by the user, it gets the 
        /// ticket base on the ticketType click by the user, but first loading the
        /// ticket page it gets all the tickets
        /// 
        /// It also use in redirecting the page after the user close the ticket or reopen
        /// </summary>
        /// <param name="ticketType"></param>
        /// <param name="messageAlert"></param>
        /// <param name="errorCount"></param>
        /// <returns></returns>

        public IActionResult Index(string messageAlert = "", 
                                    int errorCount = 0)
        {
            TicketFilterViewModel ticketFilterViewModel = new TicketFilterViewModel();
            ticketFilterViewModel.Departments = GetDepartments();  // Get All Departments
            ticketFilterViewModel.Tickets = GetAllTickets();
            // This is use in opening and closing the ticket alert message
            if (!string.IsNullOrEmpty(messageAlert) && errorCount == 0)
            {
                TempData["SuccessMessage"] = messageAlert;
            }
            else if (!string.IsNullOrEmpty(messageAlert) && errorCount > 0)
            {
                TempData["ErrorMessage"] = messageAlert;
            }

            return View(ticketFilterViewModel);
        }

        public IActionResult Filter(FilterViewModel filter)
        {
            DateTime currentDate = DateTime.Now;

            // Initialize the ViewModel with departments
            TicketFilterViewModel ticketFilterViewModel = new TicketFilterViewModel
            {
                Departments = GetDepartments(),
                Tickets = new List<TicketViewModel>() // Default empty list
            };

            // Retrieve all tickets
            List<TicketViewModel> allTickets = GetAllTickets();

            if (allTickets == null || !allTickets.Any())
            {
                return View(ticketFilterViewModel); // No tickets to display
            }

            // Start filtering
            var filteredTickets = allTickets.AsQueryable();

            if (filter.StartDate != null)
            {
                DateTime endDate = filter.EndDate ?? currentDate;
                filteredTickets = filteredTickets
                                    .Where(t => t.DateTicket >= filter.StartDate
                                             && t.DateTicket <= endDate);
            }

            if (filter.EndDate != null && filter.StartDate == null)
            {
                filteredTickets = filteredTickets
                                    .Where(t => t.DateTicket <= filter.EndDate);
            }

            if (filter.DepartmentId != null && filter.DepartmentId != -1)
            {
                filteredTickets = filteredTickets
                                    .Where(t => t.TicketDepartmentId == filter.DepartmentId);
            }

            if (filter.TicketStatus != null && filter.TicketStatus != -1)
            {
                filteredTickets = filteredTickets
                                    .Where(t => t.TicketStatus == filter.TicketStatus);
            }

            // Set the filtered tickets in the ViewModel
            ticketFilterViewModel.Tickets = filteredTickets.ToList();

            // Return the correct ViewModel
            return View(ticketFilterViewModel);
        }



        /// <summary>
        /// Get all Tickets
        /// </summary>
        /// <returns></returns>
        private List<TicketViewModel> GetAllTickets()
        {
            var tickets = (from t in _context.Tickets
                               join pl in _context.PriorityLevels on t.PriorityLevelId equals pl.Id
                               join u in _context.Users on t.UserId equals u.Id
                               join ur in _context.UserRoles on u.Id equals ur.UserId
                               join r in _context.Roles on ur.RoleId equals r.Id
                               join d in _context.Departments on r.Id equals d.RoleId
                               orderby t.TicketStatus ascending, t.DateTicket descending
                               select new TicketViewModel
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
                                   TicketBy = u.UserName,
                                   TicketDepartmentId = d.Id,
                                   TicketDepartment = d.DepartmentName
                               }).ToList();
            return tickets;
        }


        /// <summary>
        /// Get the details of the specific ticket
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int id)
        {
            // Get the details of the ticket
            TicketViewModel ticket = await GetTicketDetails(id);

            // Ticket is null
            if (ticket == null)
            {
                return RedirectToAction("Index",
                                            new
                                            {
                                                ticketType = "all",
                                                messageAlert = "Ticket not found.",
                                                errorCount = 1
                                            }
                                        );
            }
            else
            {
                ticket.TicketStatusString = Enum.GetName(typeof(TicketStatus), ticket.TicketStatus);

                // Get the Department Name the one who made the ticket
                TempData["Department"] = await (from t in _context.Tickets
                                                join u in _context.Users on t.UserId equals u.Id
                                                join ur in _context.UserRoles on u.Id equals ur.UserId
                                                join r in _context.Roles on ur.RoleId equals r.Id
                                                join d in _context.Departments on r.Id equals d.RoleId
                                                select d.DepartmentName
                                            ).FirstOrDefaultAsync();

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
                if (User.IsInRole("Admin") && ticket.OpenBy != null)
                {
                    await UpdateTicketOpenBy(id); // Await the method
                }

                // Save the data to TicketDetailsViewModel
                TicketDetailsViewModel ticketDetails = new TicketDetailsViewModel
                {
                    Ticket = ticket,
                    TicketConversation = userTicketConvo
                };

                return View(ticketDetails);
            }

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


        /// <summary>
        /// Close Ticket
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

            return RedirectToAction("Index", new {messageAlert, errorCount });
        }

        /// <summary>
        ///  Make the ticket status pending
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

            return RedirectToAction("Index", new {messageAlert, errorCount });
        }


        /// <summary>
        /// Reopen the ticket
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

            return RedirectToAction("Index", new {messageAlert, errorCount });
        }


        /// <summary>
        /// Use in downloading the file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Get departments data, used in select option in views
        /// </summary>
        /// <returns></returns>
        [NonAction]
        private List<SelectListItem> GetDepartments()
        {
            var departments = _context.Departments
                        .Select(d => new SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = d.DepartmentName
                        }).ToList();

            return departments;
        }
    }
}
