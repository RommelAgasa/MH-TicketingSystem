using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MH_TicketingSystem.ViewComponents
{
    public class AppRolesViewComponent : ViewComponent
    {

        private readonly RoleManager<IdentityRole> _roleManager;

        public AppRolesViewComponent(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public IViewComponentResult Invoke()
        {
            var roles = _roleManager.Roles;
            return View("Default", roles); // This will look for a view named Default.cshtml
        }
    }
}
