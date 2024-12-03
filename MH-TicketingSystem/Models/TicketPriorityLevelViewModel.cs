using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MH_TicketingSystem.Models
{

	// Combination of Ticket Table and PriorityLevel Table
	public class TicketPriorityLevelViewModel
	{

		// From Ticket Table
		public string TicketUserId { get; set; }
		public int TicketId { get; set; }

		public int TicketNumber { get; set; }

		public string Subject { get; set; }

		public string Description { get; set; }

		public string? FilePath { get; set; }

		public string? FileName { get; set; }

		public DateTime DateTicket { get; set; }

		public int TicketStatus { get; set; }

        public DateTime SLADeadline { get; set; } // when a ticket should be resolved based on service level agreements.


        // From Priority Level Table
        public int PriorityLevelId { get; set; }

		public string PriorityLevelName { get; set; }

		public string PriorityLevelColor { get; set; }
	}
}
