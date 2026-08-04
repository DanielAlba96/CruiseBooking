using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingCheckIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckInTokenHash",
                schema: "dbo",
                table: "Bookings",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "dbo",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CheckInTokenHash",
                schema: "dbo",
                table: "Bookings",
                column: "CheckInTokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_CheckInTokenHash",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CheckInTokenHash",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "dbo",
                table: "Bookings");
        }
    }
}
