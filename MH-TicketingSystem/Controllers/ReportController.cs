using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace MH_TicketingSystem.Controllers
{
    public class ReportController : Controller
    {

        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<ReportTicketViewModel> reportTickets = GetReportTicketViewModel();
            return View(reportTickets);
        }


        public List<ReportTicketViewModel> GetReportTicketViewModel()
        {
            var departments = _context.Departments.ToList();
            var userDepartments = _context.UserDepartments.ToList();
            var tickets = _context.Tickets.ToList();

            var report = departments
                .GroupJoin(
                    userDepartments,
                    department => department.Id,
                    userDepartment => userDepartment.DepartmentId,
                    (department, userDepartments) => new
                    {
                        Department = department,
                        Users = userDepartments.Select(ud => ud.UserId)
                    }
                )
                .SelectMany(
                    deptUsers => deptUsers.Users.DefaultIfEmpty(),
                    (deptUsers, userId) => new
                    {
                        deptUsers.Department,
                        UserId = userId
                    }
                )
                .GroupJoin(
                    tickets,
                    du => du.UserId,
                    ticket => ticket.UserId,
                    (du, tickets) => new
                    {
                        du.Department,
                        Tickets = tickets
                    }
                )
                .GroupBy(d => d.Department)
                .Select(g => new ReportTicketViewModel
                {
                    DepartmentId = g.Key.Id.ToString(),
                    Departement = $"{g.Key.DepartmentName} ({g.Key.DepartmentCode})",
                    NumberOfTicketClosed = g.SelectMany(d => d.Tickets).Count(ticket => ticket.TicketStatus == (int)TicketStatus.Closed),
                    NumberOfTicketOpen = g.SelectMany(d => d.Tickets).Count(ticket => ticket.TicketStatus == (int)TicketStatus.Open),
                    NumberOfPendingTickets = g.SelectMany(d => d.Tickets).Count(ticket => ticket.TicketStatus == (int)TicketStatus.Pending)
                })
                .OrderBy(d => d.Departement)
                .ToList();

            return report;
        }


    }
}
