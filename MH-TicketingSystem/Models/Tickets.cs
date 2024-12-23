using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MH_TicketingSystem.Models
{
    public enum TicketStatus
    {
        Open,
        Pending,
        Closed,
    }

    public class Tickets
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int TicketNumber { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Description { get; set; }


        [ValidateNever]
        public string? FilePath { get; set; }

        [ValidateNever]
        public string? FileName { get; set; }

        [Required]
        [ValidateNever]
        public DateTime DateTicket { get; set; }

        [ValidateNever]
        public DateTime? DateOpen { get; set; } // Date when the IT open the ticket

        [ValidateNever]
        public DateTime? DateClose { get; set; }

        [Required]
        [ValidateNever]
        public int TicketStatus { get; set; }

        [ValidateNever]
        public string? Resolution { get; set; } // Storing how the ticket was resolved helps with future troubleshooting or knowledge base creation.

        public DateTime SLADeadline { get; set; } // when a ticket should be resolved based on service level agreements.

        // Foreign Keys
        [ValidateNever]
        public string UserId { get; set; }    // Created by
        [ValidateNever]
        public string? OpenBy { get; set; }   // Opened by
        [ValidateNever]
        public string? CloseBy { get; set; }  // Closed by

        [Required(ErrorMessage = "Please select priority level.")]
        public int PriorityLevelId { get; set; } // Reference to PriorityLevelID


        // Navigation Properties
        [BindNever]
        [ValidateNever]
        public IdentityUser User { get; set; } // Creator (UserId)

        [BindNever]
        [ValidateNever]
        public IdentityUser OpenedByUser { get; set; } // OpenBy

        [BindNever]
        [ValidateNever]
        public IdentityUser ClosedByUser { get; set; } // CloseBy


        [BindNever]
        [ValidateNever]
        public PriorityLevel PriorityLevel { get; set; }


        [BindNever]
        [ValidateNever]
        public ICollection<TicketConversation> TicketConversations { get; set; } // One-to-many relationship
    }
    
}
