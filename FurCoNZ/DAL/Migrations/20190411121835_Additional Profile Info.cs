using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FurCoNZ.DAL.Migrations
{
    public partial class AdditionalProfileInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Allergies",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Users",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DietryRequirements",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pronouns",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allergies",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DietryRequirements",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Pronouns",
                table: "Users");
        }
    }
}
