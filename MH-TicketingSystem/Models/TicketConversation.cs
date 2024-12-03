using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MH_TicketingSystem.Models
{
    public class TicketConversation
    {
        [Required]
        public int Id { get; set; } // Primary Key

        // Foreign Keys
        [Required]
        public int TicketId { get; set; } // Reference the Tickets table

        [Required]
        public string UserID { get; set; }

        // Message Details
        [Required]
        public string Message { get; set; } // The actual conversation content

        [Required]
        public DateTime Timestamp { get; set; } // When the message was sent

        // File Attachment
        [ValidateNever]
        public string? FileName { get; set; }
        [ValidateNever]
        public string? FilePath { get; set; }

        // Navigation Properties
        [BindNever]
        [ValidateNever]
        public Tickets Tickets { get; set; }

        [BindNever]
        [ValidateNever]
        public IdentityUser User { get; set; }

    }
}