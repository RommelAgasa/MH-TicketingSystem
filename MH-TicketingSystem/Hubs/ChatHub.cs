using Azure.Core;
using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MH_TicketingSystem.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

		// User joins a group based on the TicketId
		public async Task JoinTicketGroup(int ticketId)
		{
			string groupName = $"Ticket-{ticketId}";
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
			await Clients.Group(groupName).SendAsync("Notify", $"A user has joined the ticket {ticketId}.");
		}

		// User leaves the group
		public async Task LeaveTicketGroup(int ticketId)
		{
			string groupName = $"Ticket-{ticketId}";
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
			await Clients.Group(groupName).SendAsync("Notify", $"A user has left the ticket {ticketId}.");
		}

		// Send a message to the group
		public async Task SendMessageToTicket(int ticketId, object messageData)
		{
			try
			{
				var jsonElement = (JsonElement)messageData;

				var userId = jsonElement.GetProperty("userId").GetString();
				var message = jsonElement.GetProperty("message").GetString();

				// Optional: Extract file data
				var file = jsonElement.TryGetProperty("file", out var fileElement) && fileElement.ValueKind == JsonValueKind.Object
					? new
					{
						FileName = fileElement.GetProperty("fileName").GetString(),
						FilePath = fileElement.GetProperty("filePath").GetString()
					}
					: null;

				var newConversation = new TicketConversation
				{
					TicketId = ticketId,
					UserID = userId,
					Message = message,
					FileName = file?.FileName,
					FilePath = file?.FilePath,
					Timestamp = DateTime.Now
				};

				// Save to the database
				await _context.TicketConversation.AddAsync(newConversation);
				await _context.SaveChangesAsync();

				// GroupName of conversation
				string groupName = $"Ticket-{ticketId}";

				// Notify clients
				await Clients.Group(groupName).SendAsync("ReceiveMessage", messageData);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				throw;
			}
		}

	}
}
