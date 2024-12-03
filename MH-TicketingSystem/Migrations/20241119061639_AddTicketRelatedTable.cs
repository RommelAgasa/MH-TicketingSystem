using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MH_TicketingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketRelatedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "Tickets",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        TicketNumber = table.Column<int>(type: "int", nullable: false),
            //        Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        DateTicket = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DateOpen = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        DateClose = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        TicketStatus = table.Column<int>(type: "int", nullable: false),
            //        Resolution = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        SLADeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        IsActive = table.Column<bool>(type: "bit", nullable: false),
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        OpenBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        CloseBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        PriorityLevelId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Tickets", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Tickets_AspNetUsers_CloseBy",
            //            column: x => x.CloseBy,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //        table.ForeignKey(
            //            name: "FK_Tickets_AspNetUsers_OpenBy",
            //            column: x => x.OpenBy,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //        table.ForeignKey(
            //            name: "FK_Tickets_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //        table.ForeignKey(
            //            name: "FK_Tickets_PriorityLevels_PriorityLevelId",
            //            column: x => x.PriorityLevelId,
            //            principalTable: "PriorityLevels",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "TicketConversation",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        TicketId = table.Column<int>(type: "int", nullable: false),
            //        UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TicketConversation", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_TicketConversation_AspNetUsers_UserID",
            //            column: x => x.UserID,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //        table.ForeignKey(
            //            name: "FK_TicketConversation_Tickets_TicketId",
            //            column: x => x.TicketId,
            //            principalTable: "Tickets",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_TicketConversation_TicketId",
            //    table: "TicketConversation",
            //    column: "TicketId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_TicketConversation_UserID",
            //    table: "TicketConversation",
            //    column: "UserID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Tickets_CloseBy",
            //    table: "Tickets",
            //    column: "CloseBy");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Tickets_OpenBy",
            //    table: "Tickets",
            //    column: "OpenBy");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Tickets_PriorityLevelId",
            //    table: "Tickets",
            //    column: "PriorityLevelId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Tickets_UserId",
            //    table: "Tickets",
            //    column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketConversation");

            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}
