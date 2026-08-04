using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Bookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingCabins_Bookings_BookingId",
                schema: "dbo",
                table: "BookingCabins");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingCabins_CabinTypes_CabinTypeId",
                schema: "dbo",
                table: "BookingCabins");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_CruiseDates_CruiseDateId",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_UserId",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_CruiseDateExtras_CruiseDates_CruiseDateId",
                schema: "dbo",
                table: "CruiseDateExtras");

            migrationBuilder.DropForeignKey(
                name: "FK_CruiseDateExtras_Extras_ExtraId",
                schema: "dbo",
                table: "CruiseDateExtras");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipCabins_CabinTypes_CabinTypeId",
                schema: "dbo",
                table: "ShipCabins");

            migrationBuilder.DropTable(
                name: "BookingCabinExtras",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Extras_Name",
                schema: "dbo",
                table: "Extras");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_UserId",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_BookingCabins_CabinTypeId",
                schema: "dbo",
                table: "BookingCabins");

            migrationBuilder.DropColumn(
                name: "BookingDate",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CabinTypeId",
                schema: "dbo",
                table: "BookingCabins");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "dbo",
                table: "Bookings",
                newName: "PaymenId");

            migrationBuilder.RenameColumn(
                name: "NumMinors",
                schema: "dbo",
                table: "BookingCabins",
                newName: "Occupants");

            migrationBuilder.RenameColumn(
                name: "NumAdults",
                schema: "dbo",
                table: "BookingCabins",
                newName: "CabinId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "dbo",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "BookingCabinOccupants",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingCabinId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Surname = table.Column<string>(type: "text", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Dni = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingCabinOccupants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingCabinOccupants_BookingCabins_BookingCabinId",
                        column: x => x.BookingCabinId,
                        principalSchema: "dbo",
                        principalTable: "BookingCabins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingExtras",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    ExtraId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingExtras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingExtras_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "dbo",
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingExtras_Extras_ExtraId",
                        column: x => x.ExtraId,
                        principalSchema: "dbo",
                        principalTable: "Extras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "IX_Extras_Code",
                schema: "dbo",
                table: "Extras",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PaymenId",
                schema: "dbo",
                table: "Bookings",
                column: "PaymenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingCabins_CabinId",
                schema: "dbo",
                table: "BookingCabins",
                column: "CabinId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingCabinOccupants_BookingCabinId",
                schema: "dbo",
                table: "BookingCabinOccupants",
                column: "BookingCabinId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingExtras_BookingId",
                schema: "dbo",
                table: "BookingExtras",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingExtras_ExtraId",
                schema: "dbo",
                table: "BookingExtras",
                column: "ExtraId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingCabins_Bookings_BookingId",
                schema: "dbo",
                table: "BookingCabins",
                column: "BookingId",
                principalSchema: "dbo",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingCabins_ShipCabins_CabinId",
                schema: "dbo",
                table: "BookingCabins",
                column: "CabinId",
                principalSchema: "dbo",
                principalTable: "ShipCabins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_BookingPayment_PaymenId",
                schema: "dbo",
                table: "Bookings",
                column: "PaymenId",
                principalSchema: "dbo",
                principalTable: "BookingPayment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_CruiseDates_CruiseDateId",
                schema: "dbo",
                table: "Bookings",
                column: "CruiseDateId",
                principalSchema: "dbo",
                principalTable: "CruiseDates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CruiseDateExtras_CruiseDates_CruiseDateId",
                schema: "dbo",
                table: "CruiseDateExtras",
                column: "CruiseDateId",
                principalSchema: "dbo",
                principalTable: "CruiseDates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CruiseDateExtras_Extras_ExtraId",
                schema: "dbo",
                table: "CruiseDateExtras",
                column: "ExtraId",
                principalSchema: "dbo",
                principalTable: "Extras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipCabins_CabinTypes_CabinTypeId",
                schema: "dbo",
                table: "ShipCabins",
                column: "CabinTypeId",
                principalSchema: "dbo",
                principalTable: "CabinTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingCabins_Bookings_BookingId",
                schema: "dbo",
                table: "BookingCabins");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingCabins_ShipCabins_CabinId",
                schema: "dbo",
                table: "BookingCabins");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_BookingPayment_PaymenId",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_CruiseDates_CruiseDateId",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_CruiseDateExtras_CruiseDates_CruiseDateId",
                schema: "dbo",
                table: "CruiseDateExtras");

            migrationBuilder.DropForeignKey(
                name: "FK_CruiseDateExtras_Extras_ExtraId",
                schema: "dbo",
                table: "CruiseDateExtras");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipCabins_CabinTypes_CabinTypeId",
                schema: "dbo",
                table: "ShipCabins");

            migrationBuilder.DropTable(
                name: "BookingCabinOccupants",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BookingExtras",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BookingPayment",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Extras_Code",
                schema: "dbo",
                table: "Extras");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_PaymenId",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_BookingCabins_CabinId",
                schema: "dbo",
                table: "BookingCabins");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "PaymenId",
                schema: "dbo",
                table: "Bookings",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Occupants",
                schema: "dbo",
                table: "BookingCabins",
                newName: "NumMinors");

            migrationBuilder.RenameColumn(
                name: "CabinId",
                schema: "dbo",
                table: "BookingCabins",
                newName: "NumAdults");

            migrationBuilder.AddColumn<DateOnly>(
                name: "BookingDate",
                schema: "dbo",
                table: "Bookings",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "CabinTypeId",
                schema: "dbo",
                table: "BookingCabins",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BookingCabinExtras",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingCabinId = table.Column<int>(type: "integer", nullable: false),
                    ExtraId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingCabinExtras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingCabinExtras_BookingCabins_BookingCabinId",
                        column: x => x.BookingCabinId,
                        principalSchema: "dbo",
                        principalTable: "BookingCabins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingCabinExtras_Extras_ExtraId",
                        column: x => x.ExtraId,
                        principalSchema: "dbo",
                        principalTable: "Extras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Dni = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    KeycloakId = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    RegisterDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Extras_Name",
                schema: "dbo",
                table: "Extras",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                schema: "dbo",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingCabins_CabinTypeId",
                schema: "dbo",
                table: "BookingCabins",
                column: "CabinTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingCabinExtras_BookingCabinId",
                schema: "dbo",
                table: "BookingCabinExtras",
                column: "BookingCabinId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingCabinExtras_ExtraId",
                schema: "dbo",
                table: "BookingCabinExtras",
                column: "ExtraId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Dni",
                schema: "dbo",
                table: "Users",
                column: "Dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "dbo",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_KeycloakId",
                schema: "dbo",
                table: "Users",
                column: "KeycloakId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Phone",
                schema: "dbo",
                table: "Users",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                schema: "dbo",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingCabins_Bookings_BookingId",
                schema: "dbo",
                table: "BookingCabins",
                column: "BookingId",
                principalSchema: "dbo",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingCabins_CabinTypes_CabinTypeId",
                schema: "dbo",
                table: "BookingCabins",
                column: "CabinTypeId",
                principalSchema: "dbo",
                principalTable: "CabinTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_CruiseDates_CruiseDateId",
                schema: "dbo",
                table: "Bookings",
                column: "CruiseDateId",
                principalSchema: "dbo",
                principalTable: "CruiseDates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_UserId",
                schema: "dbo",
                table: "Bookings",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CruiseDateExtras_CruiseDates_CruiseDateId",
                schema: "dbo",
                table: "CruiseDateExtras",
                column: "CruiseDateId",
                principalSchema: "dbo",
                principalTable: "CruiseDates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CruiseDateExtras_Extras_ExtraId",
                schema: "dbo",
                table: "CruiseDateExtras",
                column: "ExtraId",
                principalSchema: "dbo",
                principalTable: "Extras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipCabins_CabinTypes_CabinTypeId",
                schema: "dbo",
                table: "ShipCabins",
                column: "CabinTypeId",
                principalSchema: "dbo",
                principalTable: "CabinTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
