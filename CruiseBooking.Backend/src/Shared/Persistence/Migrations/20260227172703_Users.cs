using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Users : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_BookingPayment_PaymenId",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "BookingPayment",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_PaymenId",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "PaymenId",
                schema: "dbo",
                table: "Bookings",
                newName: "UserId");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KeycloakId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Surname = table.Column<string>(type: "text", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Dni = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Nationality = table.Column<string>(type: "text", nullable: false),
                    CardNumber = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                schema: "dbo",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Dni",
                schema: "dbo",
                table: "Users",
                column: "Dni");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "dbo",
                table: "Users",
                column: "Email");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_UserId",
                schema: "dbo",
                table: "Bookings",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_UserId",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_UserId",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "dbo",
                table: "Bookings",
                newName: "PaymenId");

            migrationBuilder.CreateTable(
                name: "BookingPayment",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CardHolderName = table.Column<string>(type: "text", nullable: false),
                    CardNumber = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingPayment", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PaymenId",
                schema: "dbo",
                table: "Bookings",
                column: "PaymenId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_BookingPayment_PaymenId",
                schema: "dbo",
                table: "Bookings",
                column: "PaymenId",
                principalSchema: "dbo",
                principalTable: "BookingPayment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
