using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FurCoNZ.Web.DAL.Migrations
{
    public partial class TicketTypeExpiry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SoldOutAt",
                table: "TicketTypes",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoldOutAt",
                table: "TicketTypes");
        }
    }
}
