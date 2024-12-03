using Microsoft.AspNetCore.Identity;

namespace MH_TicketingSystem.Models
{
    public class SettingViewModel
    {
        public IdentityRole Role { get; set; }
        public PriorityLevel PriorityLevel { get; set; }
    }
}
