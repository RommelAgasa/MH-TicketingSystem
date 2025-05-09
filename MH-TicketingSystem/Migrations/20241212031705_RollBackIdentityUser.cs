﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MH_TicketingSystem.Migrations
{
    /// <inheritdoc />
    public partial class RollBackIdentityUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_AspNetUserRoles_AspNetUsers_ApplicationUserId",
            //    table: "AspNetUserRoles");

            //migrationBuilder.DropIndex(
            //    name: "IX_AspNetUserRoles_ApplicationUserId",
            //    table: "AspNetUserRoles");

            //migrationBuilder.DropColumn(
            //    name: "ApplicationUserId",
            //    table: "AspNetUserRoles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "AspNetUserRoles",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_ApplicationUserId",
                table: "AspNetUserRoles",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_ApplicationUserId",
                table: "AspNetUserRoles",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
