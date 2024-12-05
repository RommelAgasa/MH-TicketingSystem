using MH_TicketingSystem.Models;
using Microsoft.AspNetCore.SignalR;

namespace MH_TicketingSystem.Hubs
{
    public class DashboardHub : Hub
    {
        public async Task UpdateDashboard(string metric, int value)
        {
            await Clients.All.SendAsync("UpdateMetric", metric, value);
        }

        public async Task NewTicketAdded(TicketPriorityLevelViewModel ticket,
                string ticketStatus, string priorityLevelColor)
        {
            await Clients.All.SendAsync("ReceiveNewTicket", ticket, ticketStatus, priorityLevelColor);
        }
    }
}
