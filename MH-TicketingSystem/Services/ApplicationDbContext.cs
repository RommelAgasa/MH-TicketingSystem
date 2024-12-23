using MH_TicketingSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.Sockets;

namespace MH_TicketingSystem.Services
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        /**
         * In ASP.NET Core, only models that are part of your Entity 
         * Framework Core (EF Core) DbContext and configured with DbSet 
         * properties will be included in migrations.
         */

        public DbSet<Tickets> Tickets { get; set; }

        public DbSet<TicketConversation> TicketConversation { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<PriorityLevel> PriorityLevels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship 
            // Department Table
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Role)
                .WithMany()
                .HasForeignKey(d => d.RoleId);


            // Ticket Table
            // UserId (Creator)
            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // OpenBy (Opened by user)
            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.OpenedByUser)
                .WithMany()
                .HasForeignKey(t => t.OpenBy)
                .OnDelete(DeleteBehavior.Restrict);

            // CloseBy (Closed by user)
            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.ClosedByUser)
                .WithMany()
                .HasForeignKey(t => t.CloseBy)
                .OnDelete(DeleteBehavior.Restrict);

            // PriorityLevel
            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.PriorityLevel)
                .WithMany()
                .HasForeignKey(t => t.PriorityLevelId)
                .OnDelete(DeleteBehavior.Restrict);


            // TicketConversation
            // TicketID
            modelBuilder.Entity<TicketConversation>()
                .HasOne(tc => tc.Ticket)
                .WithMany()
                .HasForeignKey(tc => tc.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserID
            modelBuilder.Entity<TicketConversation>()
                .HasOne(tc => tc.User)
                .WithMany()
                .HasForeignKey(tc => tc.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure one-to-many relationship between Tickets and TicketConversation
            modelBuilder.Entity<TicketConversation>()
                .HasOne(tc => tc.Ticket)
                .WithMany(t => t.TicketConversations)
                .HasForeignKey(tc => tc.TicketId)
                .OnDelete(DeleteBehavior.Cascade); // Specify delete behavior if needed
        }

    }
}
