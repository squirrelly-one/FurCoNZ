using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FurCoNZ.Web.DAL.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    PriceCents = table.Column<int>(nullable: false),
                    TotalAvailable = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    IsAdmin = table.Column<bool>(nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LinkedAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Issuer = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkedAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkedAccounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderedById = table.Column<int>(nullable: false),
                    AmountPaidCents = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_OrderedById",
                        column: x => x.OrderedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderAudits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(nullable: false),
                    AmountCents = table.Column<int>(nullable: false),
                    When = table.Column<DateTimeOffset>(nullable: false),
                    PaymentProvider = table.Column<string>(nullable: false),
                    PaymentProviderReference = table.Column<string>(nullable: false),
                    OrderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderAudits_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StripeSessions",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    PaymentIntent = table.Column<string>(nullable: true),
                    OrderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripeSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StripeSessions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttendeeAccountId = table.Column<int>(nullable: true),
                    OrderId = table.Column<int>(nullable: false),
                    TicketTypeId = table.Column<int>(nullable: false),
                    TicketName = table.Column<string>(nullable: true),
                    PreferredName = table.Column<string>(nullable: true),
                    PreferredPronouns = table.Column<string>(nullable: true),
                    LegalName = table.Column<string>(nullable: true),
                    DateOfBirth = table.Column<DateTime>(nullable: false),
                    EmailAddress = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Suburb = table.Column<string>(nullable: true),
                    CityTown = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    MealRequirements = table.Column<int>(nullable: false),
                    KnownAllergens = table.Column<string>(nullable: true),
                    CabinGrouping = table.Column<string>(nullable: true),
                    CabinNoisePreference = table.Column<int>(nullable: false),
                    AdditionalNotes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_AttendeeAccountId",
                        column: x => x.AttendeeAccountId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_TicketTypes_TicketTypeId",
                        column: x => x.TicketTypeId,
                        principalTable: "TicketTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LinkedAccounts_UserId",
                table: "LinkedAccounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedAccounts_Issuer_Subject",
                table: "LinkedAccounts",
                columns: new[] { "Issuer", "Subject" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderAudits_OrderId",
                table: "OrderAudits",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAudits_PaymentProvider_PaymentProviderReference",
                table: "OrderAudits",
                columns: new[] { "PaymentProvider", "PaymentProviderReference" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderedById",
                table: "Orders",
                column: "OrderedById");

            migrationBuilder.CreateIndex(
                name: "IX_StripeSessions_OrderId",
                table: "StripeSessions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_StripeSessions_PaymentIntent",
                table: "StripeSessions",
                column: "PaymentIntent");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AttendeeAccountId",
                table: "Tickets",
                column: "AttendeeAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrderId",
                table: "Tickets",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketTypeId",
                table: "Tickets",
                column: "TicketTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LinkedAccounts");

            migrationBuilder.DropTable(
                name: "OrderAudits");

            migrationBuilder.DropTable(
                name: "StripeSessions");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "TicketTypes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
