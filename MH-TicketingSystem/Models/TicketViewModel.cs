using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MH_TicketingSystem.Models
{

    // Combination of Ticket Table and PriorityLevel Table
    public class TicketViewModel
    {

        // From Ticket Table
        public string TicketUserId { get; set; }

        public string? TicketBy {  get; set; }

        public int TicketId { get; set; }
        public int TicketReplies { get; set; }

        public int TicketNumber { get; set; }

        public string Subject { get; set; }

        public string Description { get; set; }

        public string TicketDepartment { get; set; }

        public int TicketDepartmentId { get; set; }

        public string? OpenBy { get; set; } = null;

        public DateTime? OpenDateTime { get; set; } = null;

        public string? ClosedBy { get; set; } = null;

        public DateTime? ClosedDateTime { get; set; } = null;

        public string? FilePath { get; set; }

        public string? FileName { get; set; }

        public DateTime DateTicket { get; set; }

        public int TicketStatus { get; set; }

        public string TicketStatusString { get; set; }

        public DateTime? SLADeadline { get; set; } // when a ticket should be resolved based on service level agreements.

        // From Priority Level Table
        public int PriorityLevelId { get; set; }

        public string PriorityLevelName { get; set; }

        public string PriorityLevelColor { get; set; }

        public string? Resolution { get; set; } // Storing how the ticket was resolved helps with future troubleshooting or knowledge base creation.

    }
}
