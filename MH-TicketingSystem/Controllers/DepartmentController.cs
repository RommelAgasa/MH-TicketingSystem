using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MH_TicketingSystem.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DepartmentController(RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Roles = await _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Id
                }).ToListAsync();
            return View();
        }


        // Get all departments
        [HttpGet]
        public JsonResult GetAllDepartments()
        {
            var departments = _context.Departments.ToList();
            return Json(departments);
        }

        // Create a new department
        [HttpPost]
        public JsonResult Create(Department department)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Model validation failed." });
            }

            try
            {
                _context.Add(department);
                _context.SaveChanges();
                return Json(new { success = true, message = "Department details saved." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error saving department: {ex.Message}" });
            }
        }

        // Get department for editing
        [HttpGet]
        public JsonResult Edit(int id)
        {
            var department = _context.Departments.Find(id);
            if (department == null)
            {
                return Json(new { success = false, message = $"Department not found with id {id}" });
            }
            return Json(new { success = true, department });
        }

        // Update department details
        [HttpPost]
        public JsonResult Update(Department department)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Model validation failed." });
            }

            try
            {
                _context.Departments.Update(department);
                _context.SaveChanges();
                return Json(new { success = true, message = "Department details updated." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error updating department: {ex.Message}" });
            }
        }

        // Delete a department
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var department = _context.Departments.Find(id);
            if (department == null)
            {
                return Json(new { success = false, message = $"Department not found with id {id}" });
            }

            try
            {
                // check if there is a record in ticket table
                var hasRecord = true;

                if (hasRecord)
                {
                    return Json(new { success = false, message = "This department is referenced in other data and cannot be removed." });
                }

                _context.Departments.Remove(department);
                _context.SaveChanges();
                return Json(new { success = true, message = "Department details deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting department: {ex.Message}" });
            }
        }


    }
}
