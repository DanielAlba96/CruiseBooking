using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTokenHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentTokenHash",
                schema: "dbo",
                table: "Bookings",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PaymentTokenHash",
                schema: "dbo",
                table: "Bookings",
                column: "PaymentTokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_PaymentTokenHash",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaymentTokenHash",
                schema: "dbo",
                table: "Bookings");
        }
    }
}
