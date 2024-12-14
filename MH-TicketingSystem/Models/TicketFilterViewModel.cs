using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MH_TicketingSystem.Models
{
    public class TicketFilterViewModel
    {
        public List<TicketViewModel> Tickets { get; set; }
        public FilterViewModel Filter { get; set; }

        [ValidateNever]
        public List<SelectListItem>? Departments { get; set; }
    }
}
