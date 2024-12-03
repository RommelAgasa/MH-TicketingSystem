using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MH_TicketingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorityLevelTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "PriorityLevels",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        PriorityLevelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PriorityLevelDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PriorityLevelColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IsPriorityLevelActive = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PriorityLevels", x => x.Id);
            //    });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriorityLevels");
        }
    }
}
