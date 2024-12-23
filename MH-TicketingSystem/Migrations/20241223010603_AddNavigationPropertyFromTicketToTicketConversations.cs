using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MH_TicketingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationPropertyFromTicketToTicketConversations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_TicketConversation_Tickets_TicketId",
            //    table: "TicketConversation");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_TicketConversation_Tickets_TicketId",
            //    table: "TicketConversation",
            //    column: "TicketId",
            //    principalTable: "Tickets",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketConversation_Tickets_TicketId",
                table: "TicketConversation");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketConversation_Tickets_TicketId",
                table: "TicketConversation",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
