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

        //public List<ReportTicketViewModel> GetReportTicketViewModel()
        //{
        //    var report = _context.Departments
        //        .GroupJoin(
        //            _context.Roles,
        //            department => department.RoleId,
        //            role => role.Id,
        //            (department, roles) => new { Department = department, Roles = roles }
        //        )
        //        .SelectMany(
        //            dr => dr.Roles.DefaultIfEmpty(),
        //            (dr, role) => new { dr.Department, Role = role }
        //        )
        //        .GroupJoin(
        //            _context.UserRoles,
        //            dr => dr.Role.Id,
        //            userRole => userRole.RoleId,
        //            (dr, userRoles) => new { dr.Department, UserRoles = userRoles }
        //        )
        //        .SelectMany(
        //            dru => dru.UserRoles.DefaultIfEmpty(),
        //            (dru, userRole) => new { dru.Department, UserRole = userRole }
        //        )
        //        .GroupJoin(
        //            _context.Users,
        //            dru => dru.UserRole.UserId,
        //            user => user.Id,
        //            (dru, users) => new { dru.Department, Users = users }
        //        )
        //        .SelectMany(
        //            du => du.Users.DefaultIfEmpty(),
        //            (du, user) => new { du.Department, User = user }
        //        )
        //        .GroupJoin(
        //            _context.Tickets,
        //            du => du.User.Id,
        //            ticket => ticket.UserId,
        //            (du, tickets) => new { du.Department, Tickets = tickets }
        //        )
        //        .AsEnumerable() // Switch to client-side evaluation to handle grouping and counting
        //        .Select(d => new ReportTicketViewModel
        //        {
        //            DepartmentId = d.Department.Id.ToString(),
        //            Departement = $"{d.Department.DepartmentName} ({d.Department.DepartmentCode})",
        //            NumberOfTicketClosed = d.Tickets.Count(ticket => ticket.TicketStatus == (int)TicketStatus.Closed),
        //            NumberOfTicketOpen = d.Tickets.Count(ticket => ticket.TicketStatus == (int)TicketStatus.Open),
        //            NumberOfPendingTickets = d.Tickets.Count(ticket => ticket.TicketStatus == (int)TicketStatus.Pending)
        //        })
        //        .ToList();

        //    return report;
        //}


        public List<ReportTicketViewModel> GetReportTicketViewModel()
        {
            var report = _context.Departments
                .GroupJoin(
                    _context.Roles,
                    department => department.RoleId,
                    role => role.Id,
                    (department, roles) => new { Department = department, Roles = roles }
                )
                .SelectMany(
                    dr => dr.Roles.DefaultIfEmpty(),
                    (dr, role) => new { dr.Department, Role = role }
                )
                .GroupJoin(
                    _context.UserRoles,
                    dr => dr.Role.Id,
                    userRole => userRole.RoleId,
                    (dr, userRoles) => new { dr.Department, UserRoles = userRoles }
                )
                .SelectMany(
                    dru => dru.UserRoles.DefaultIfEmpty(),
                    (dru, userRole) => new { dru.Department, UserRole = userRole }
                )
                .GroupJoin(
                    _context.Users,
                    dru => dru.UserRole.UserId,
                    user => user.Id,
                    (dru, users) => new { dru.Department, Users = users }
                )
                .SelectMany(
                    du => du.Users.DefaultIfEmpty(),
                    (du, user) => new { du.Department, User = user }
                )
                .GroupJoin(
                    _context.Tickets,
                    du => du.User.Id, // Handle null users
                    ticket => ticket.UserId,
                    (du, tickets) => new { du.Department, Tickets = tickets }
                )
                .AsEnumerable() // Switch to client-side evaluation for grouping and counting
                .GroupBy(d => d.Department) // Group by department
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
