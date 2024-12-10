using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MH_TicketingSystem.Controllers
{
    /// <summary>
    /// This controller use in managing department
    /// </summary>
    [Authorize]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DepartmentController(RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _context = context;
            _roleManager = roleManager;
        }


        /// <summary>
        /// Use loading the department page
        /// Get all role using the _roleManager - use in displaying the UI element select
        /// </summary>
        /// <returns></returns>
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


        /// <summary>
        /// Get all departments
        /// </summary>
        /// <returns>JSON List of Departments</returns>
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

        /// <summary>
        /// Get department for editing
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Update department details
        /// </summary>
        /// <param name="department"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Delete a department
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
