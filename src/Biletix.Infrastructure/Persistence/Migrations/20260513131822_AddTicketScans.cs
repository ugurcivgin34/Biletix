using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biletix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketScans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketScans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScannedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    InvalidReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketScans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketScans_BookingId",
                table: "TicketScans",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketScans_EventId",
                table: "TicketScans",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketScans_ScannedAt",
                table: "TicketScans",
                column: "ScannedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketScans");
        }
    }
}
