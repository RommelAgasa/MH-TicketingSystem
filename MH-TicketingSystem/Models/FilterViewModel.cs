using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MH_TicketingSystem.Models
{
    public class FilterViewModel
    {
        public int? DepartmentId { get; set; }
        public int? TicketStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
