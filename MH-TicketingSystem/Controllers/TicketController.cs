using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MH_TicketingSystem.Controllers
{
    public class TicketController : Controller
    {

        private readonly ApplicationDbContext _context;

        public TicketController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
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
			return View(userTicketConvo);
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
