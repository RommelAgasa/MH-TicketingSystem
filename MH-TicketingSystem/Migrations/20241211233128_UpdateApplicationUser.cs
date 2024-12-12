using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MH_TicketingSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "SLADeadline",
            //    table: "Tickets",
            //    type: "datetime2",
            //    nullable: false,
            //    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            //    oldClrType: typeof(DateTime),
            //    oldType: "datetime2",
            //    oldNullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "ApplicationUserId",
            //    table: "AspNetUserRoles",
            //    type: "nvarchar(450)",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserRoles_ApplicationUserId",
            //    table: "AspNetUserRoles",
            //    column: "ApplicationUserId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AspNetUserRoles_AspNetUsers_ApplicationUserId",
            //    table: "AspNetUserRoles",
            //    column: "ApplicationUserId",
            //    principalTable: "AspNetUsers",
            //    principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_ApplicationUserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_ApplicationUserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "AspNetUserRoles");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SLADeadline",
                table: "Tickets",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
