using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MH_TicketingSystem.Models
{
    public class Department
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string DepartmentName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string DepartmentCode { get; set; }

        public Boolean IsDepartmentActive { get; set; }

        [Required]
        public string RoleId { get; set; } // reference this id to the table dbo.AspNetRoles

        // Navigation Property
        [BindNever]
        [ValidateNever]
        public virtual IdentityRole Role { get; set; }

    }
}
