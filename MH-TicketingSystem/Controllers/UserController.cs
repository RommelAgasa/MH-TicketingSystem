using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MH_TicketingSystem.Controllers
{
    /// <summary>
    /// This controller use in managing the user - deleting and updating the data information
    /// </summary>
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Use in loading the users in users page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var users = GetUsers();
            return View(users ?? new List<UserDepartmentViewModel>());
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        public List<UserDepartmentViewModel> GetUsers()
        {
            var users =  (from ur in _context.UserRoles
                         join u in _context.Users on ur.UserId equals u.Id
                         join r in _context.Roles on ur.RoleId equals r.Id
                         join d in _context.Departments on r.Id equals d.RoleId
                         select new UserDepartmentViewModel
                         {
                             UserId = u.Id,
                             UserName = u.UserName,
                             Password = u.PasswordHash,
                             DepartmentId = d.Id,
                             DepartmentName = d.DepartmentName
                         }).ToList();

            return users;
        }


        /// <summary>
        ///  Edit the user data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(string id) 
        {
            if (id == null || string.IsNullOrEmpty(id))
            {
                return View("Index");
            }

            var userEdit = GetUsers().Where(x => x.UserId == id).FirstOrDefault();

            ViewBag.Departments = GetDepartments();

            return View(userEdit);
        }


        /// <summary>
        /// Submitting the edited data of the user
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string UserId, UserDepartmentViewModel userModel)
        {
            ViewBag.Departments = GetDepartments();
            // Find user by ID
            var user = await _userManager.FindByIdAsync(userModel.UserId);
            if(user == null)
            {
                // Log error and return Not Found
                ModelState.AddModelError("", "User not found.");
                return NotFound();
            }

            // Change Password
            if (!string.IsNullOrEmpty(userModel.Password))
            {
                // Generate a password reset token for the user
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resultChangePassword = await _userManager
                        .ResetPasswordAsync(user, resetToken, userModel.Password);
                if (!resultChangePassword.Succeeded)
                {
                    // Log password change errors
                    foreach (var error in resultChangePassword.Errors)
                    {
                        ModelState.AddModelError("", $"Password Error: {error.Description}");
                    }
                    return View(userModel);
                }
            }

            // Update Username
            user.UserName = userModel.UserName;
            var resultChangeUsername = await _userManager.UpdateAsync(user);
            if (!resultChangeUsername.Succeeded)
            {
                foreach (var error in resultChangeUsername.Errors)
                {
                    ModelState.AddModelError("", $"Username Error: {error.Description}");
                }
                return View(userModel);
            }

            // Get RoleId from Departments table based on DepartmentId
            var department = await _context.Departments.FindAsync(userModel.DepartmentId);
            if (department == null)
            {
                ModelState.AddModelError("", "Department not found.");
                return View(userModel);
            }

            // Get RoleName
            string roleId = department.RoleId.ToString();

            // Get the role by roleId
            var userRole = await _context.Roles.FindAsync(roleId);
            if (userRole == null)
            {
                ModelState.AddModelError("", "Role not found for the specified Department.");
                return View(userModel);
            }

            var roleName = userRole.Name;

            // Get current role (since it's one-to-one)
            var currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            // Check if role update is needed
            if (currentRole != roleName)
            {
                // Remove the current role, if any
                if (!string.IsNullOrEmpty(currentRole))
                {
                    var removeResult = await _userManager.RemoveFromRoleAsync(user, currentRole);
                    if (!removeResult.Succeeded)
                    {
                        foreach (var error in removeResult.Errors)
                        {
                            ModelState.AddModelError("", $"Role Removal Error: {error.Description}");
                        }
                        return View(userModel);
                    }
                    else
                    {
                        // If succeeded add the new role of the user
                        var addResult = await _userManager.AddToRoleAsync(user, roleName);
                        if (!addResult.Succeeded)
                        {
                            foreach(var error in addResult.Errors)
                            {
                                ModelState.AddModelError("", $"Role Addition Error: {error.Description}");
                            }
                            
                            return View(userModel);
                        }
                    }
                }
            }

            TempData["Success"] = "Update account profile successfully.";
            return RedirectToAction("Index");
        }


        /// <summary>
        ///  Deleting specific user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> Delete(string id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "The user ID is empty." });
            }

            // Await the FindByIdAsync call to retrieve the user correctly
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Check if this user a reference record to department table
            var hasRecord = true;
            if (hasRecord)
            {
                return Json(new { success = true, message = "This user is referenced in other data and cannot be removed." });
            }

            // Attempt to delete the user
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "User successfully deleted." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete user.", errors = result.Errors });
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
