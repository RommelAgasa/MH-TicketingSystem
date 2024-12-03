using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MH_TicketingSystem.Attributes
{
    public class AccessDeniedAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _role;

        public AccessDeniedAuthorizeAttribute(string role)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Check if user is authenticated and has the required role
            if (!user.Identity.IsAuthenticated || !user.IsInRole(_role))
            {
                // Redirect to the custom access denied page
                context.Result = new RedirectToActionResult("Index", "AccessDenied", null);
            }
        }
    }
}
