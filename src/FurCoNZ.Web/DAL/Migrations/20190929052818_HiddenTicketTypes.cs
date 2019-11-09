using Microsoft.EntityFrameworkCore.Migrations;

namespace FurCoNZ.Web.DAL.Migrations
{
    public partial class HiddenTicketTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HiddenFromPublic",
                table: "TicketTypes",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HiddenFromPublic",
                table: "TicketTypes");
        }
    }
}
