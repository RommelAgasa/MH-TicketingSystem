using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using SQLitePCL;
using Microsoft.AspNetCore.Authorization;
using MH_TicketingSystem.Services;

namespace MH_TicketingSystem.Controllers
{

    /// <summary>
    /// This controller use in managing roles of each department - CRUD functionality
    /// </summary>
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RolesController(RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        /// <summary>
        ///  Use in getting all roles
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetRoles()
        {
            var roles = _roleManager.Roles.ToList().OrderBy(r => r.Name);
            return Json(roles);
        }

        /// <summary>
        /// Use in creating new role
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> Create(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                try
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        return Json(new { success = true, message = "New role has been saved." });
                    }
                    else
                    {
                        // Collect errors if the role creation failed
                        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                        return Json(new { success = false, message = $"Error saving new role: {errors}" });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Error saving new role: {ex.Message}" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Invalid role name." });
            }
        }

        /// <summary>
        /// Use in getting specific role to be edit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if(role == null)
            {
                return Json(new { success = false, message = $"Role not found with id {id}" });
            }
            return Json(new { success = true, role });
        }

        /// <summary>
        /// Use in updating specific role
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> Update(IdentityRole role)
        {

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Model validation failed." });
            }

            var existingRole = await _roleManager.FindByIdAsync(role.Id);
            if (existingRole != null)
            {

                if(existingRole.Name != role.Name)
                {
                    try
                    {
                        existingRole.Name = role.Name;
                        var result = await _roleManager.UpdateAsync(existingRole);
                        if (result.Succeeded)
                        {
                            return Json(new { success = true, message = "Role details updated successfully." });
                        }
                        else
                        {
                            // Collect errors
                            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                            return Json(new { success = false, message = $"Error updating role: {errors}" });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new { succcess = false, message = $"Error updating role: {ex.Message}" });
                    }
                }

                return Json(new { success = true, message = "Role details updated successfully." });
            }
            else
            {
                return Json(new { success = true, message = "Cannot find the role." });
            }
        }

        /// <summary>
        /// Use in deleting specific role
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { succes = false, message = "The data ID is empty." });
            }

            // Check if the role has been used in other table
            var isUsed = _context.Departments.Any(r => r.RoleId == id);
            if (isUsed)
            {
                return Json(new { succes = false, message = "This cannot be deleted since this data has been used in other record." });
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "Deleted successfully." });
                }
                else
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, message = $"Errors deleting role: {errors}" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Unable to find role." });
            }
        }

    }
}
