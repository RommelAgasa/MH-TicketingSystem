using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MH_TicketingSystem.Models
{
    public class PriorityLevel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string PriorityLevelName { get; set; }

        [Required]
        public string PriorityLevelDescription { get; set; }

        [Required]
        public string PriorityLevelColor { get; set; }

        [ValidateNever]
        public Boolean IsPriorityLevelActive { get; set; }
    }
}
