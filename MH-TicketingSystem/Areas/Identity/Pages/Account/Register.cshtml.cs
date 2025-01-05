// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MH_TicketingSystem.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using MH_TicketingSystem.Services;

namespace MH_TicketingSystem.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            /// 
            //[Required]
            //[EmailAddress]
            //[Display(Name = "Email")]
            //public string Email { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Username")]
            public string Username { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string? Role { get; set; } // role ID


            //[ValidateNever]
            //public IEnumerable<SelectListItem> RoleList { get; set; } // Use in UI

            [Required]
            public int DepartmentID { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> DepartmentList { get; set; }


        }
        private void PopulateDepartmentList()
        {
            Input = new InputModel
            {
                //RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                //{
                //    Text = i, // Display text in dropdown
                //    Value = i // Value that will be submitted on form
                //})

                DepartmentList = _context.Departments.Where(d => d.IsDepartmentActive == true).Select(d => new SelectListItem
                {
                    Text = d.DepartmentName,
                    Value = d.Id.ToString()
                })
            };
        }



        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            Input = new InputModel(); // Ensure Input is initialized
            PopulateDepartmentList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
            if (ModelState.IsValid)
            {

                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Username, CancellationToken.None);
                // await _emailStore.SetEmailAsync(user, Input.Username, CancellationToken.None);

                
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");


                    // Get the roleID from department 
                    var department = await _context.Departments.FindAsync(Input.DepartmentID);
                    if (department == null)
                    {
                        ModelState.AddModelError(string.Empty, "Department not found.");
                        PopulateDepartmentList();
                        return Page();
                    }

                    // Check if department and its RoleId are valid
                    string roleID = department?.RoleId; // RoleId of the department
                    if (!string.IsNullOrEmpty(roleID))
                    {
                        // Find the role by its ID
                        var role = await _roleManager.FindByIdAsync(roleID);

                        if (role != null)
                        {
                            // Add the user to the role using the role's name
                            await _userManager.AddToRoleAsync(user, role.Name);

                            await _signInManager.SignInAsync(user, isPersistent: false);

                            /**
                             * This line redirects the user to a specified URL. LocalRedirect 
                             * ensures that the URL is local to the application, preventing open 
                             * redirect vulnerabilities.
                             */

                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            // Handle the case where the role is not found
                            ModelState.AddModelError(string.Empty, "Role not found.");
                        }
                    }
                    else
                    {
                        // Handle the case where RoleId is null or empty
                        ModelState.AddModelError(string.Empty, "Department role is invalid.");
                    }


                    //var userId = await _userManager.GetUserIdAsync(user);
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    //var callbackUrl = Url.Page(
                    //    "/Account/ConfirmEmail",
                    //    pageHandler: null,
                    //    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    //    protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    //if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    //{
                    //    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    //}
                    //else
                    //{
                    //    await _signInManager.SignInAsync(user, isPersistent: false);
                    //    return LocalRedirect(returnUrl);
                    //}

                    /**
                     * _signInManager is an instance of SignInManager<TUser>, 
                     * where TUser is typically your user class (like ApplicationUser). 
                     * This class is responsible for managing user sign-ins and sign-outs, 
                     * including handling authentication cookies.
                     */

                    /**
                     * isPersistent: This boolean parameter indicates 
                     * whether the authentication cookie should persist across browser sessions.
                     */

                    
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            PopulateDepartmentList();
            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
