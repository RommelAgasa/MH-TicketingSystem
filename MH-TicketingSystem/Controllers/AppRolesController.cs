using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MH_TicketingSystem.Controllers
{

    /// <summary>
    /// This controller use in creating role and use in creating department
    /// </summary>
    [Authorize]
    public class AppRolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public AppRolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        /// <summary>
        /// Use in submitting or creating new role in the system
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            /**
             * .GetAwaiter().GetResult(): 
             * this is used to block the thread until the role creation is complete.
             */
            // Avoid duplicate role
            if (!_roleManager.RoleExistsAsync(model.Name).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(model.Name)).GetAwaiter().GetResult();
            }
            return RedirectToAction("Index", "Setting");
        }
    }
}
