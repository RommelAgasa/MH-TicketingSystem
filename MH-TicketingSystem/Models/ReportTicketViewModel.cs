namespace MH_TicketingSystem.Models
{
    public class ReportTicketViewModel
    {
        public string DepartmentId { get; set; }
        public string Departement { get; set; }
        public int NumberOfTicketClosed { get; set; }
        public int NumberOfTicketOpen { get; set; }
        public int NumberOfPendingTickets { get; set; }
    }

}
