using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace MH_TicketingSystem.Models
{
    public class UserDepartment
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        // Navigation Property
        [BindNever]
        [ValidateNever]
        public IdentityUser User { get; set; }

        [BindNever]
        [ValidateNever]
        public Department Department { get; set; }
    }
}
