using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MH_TicketingSystem.Models
{
    public class UserDepartmentViewModel
    {
        public string UserId { get; set; }

        [Required]
        public string UserName { get; set; }

        [ValidateNever]
        public string Password { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [ValidateNever]
        public string DepartmentName { get; set; }
    }
}
