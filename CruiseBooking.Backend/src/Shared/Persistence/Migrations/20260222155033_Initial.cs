using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "CabinTypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    MaxOccupancy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CabinTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cruises",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Zone = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OriginPort = table.Column<string>(type: "text", nullable: false),
                    Itinerary = table.Column<string>(type: "text", nullable: false),
                    DurationInDays = table.Column<int>(type: "integer", nullable: false),
                    AllowsMinors = table.Column<bool>(type: "boolean", nullable: false),
                    Featured = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cruises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Extras",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ships",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Company = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KeycloakId = table.Column<string>(type: "text", nullable: false),
                    RegisterDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Dni = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CruiseDates",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CruiseId = table.Column<int>(type: "integer", nullable: false),
                    ShipId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PriceModifier = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CruiseDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CruiseDates_Cruises_CruiseId",
                        column: x => x.CruiseId,
                        principalSchema: "dbo",
                        principalTable: "Cruises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CruiseDates_Ships_ShipId",
                        column: x => x.ShipId,
                        principalSchema: "dbo",
                        principalTable: "Ships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShipCabins",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShipId = table.Column<int>(type: "integer", nullable: false),
                    CabinTypeId = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipCabins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipCabins_CabinTypes_CabinTypeId",
                        column: x => x.CabinTypeId,
                        principalSchema: "dbo",
                        principalTable: "CabinTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShipCabins_Ships_ShipId",
                        column: x => x.ShipId,
                        principalSchema: "dbo",
                        principalTable: "Ships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CruiseDateId = table.Column<int>(type: "integer", nullable: false),
                    BookingDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_CruiseDates_CruiseDateId",
                        column: x => x.CruiseDateId,
                        principalSchema: "dbo",
                        principalTable: "CruiseDates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CruiseDateExtras",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CruiseDateId = table.Column<int>(type: "integer", nullable: false),
                    ExtraId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CruiseDateExtras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CruiseDateExtras_CruiseDates_CruiseDateId",
                        column: x => x.CruiseDateId,
                        principalSchema: "dbo",
                        principalTable: "CruiseDates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CruiseDateExtras_Extras_ExtraId",
                        column: x => x.ExtraId,
                        principalSchema: "dbo",
                        principalTable: "Extras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingCabins",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    CabinTypeId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NumAdults = table.Column<int>(type: "integer", nullable: false),
                    NumMinors = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingCabins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingCabins_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "dbo",
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingCabins_CabinTypes_CabinTypeId",
                        column: x => x.CabinTypeId,
                        principalSchema: "dbo",
                        principalTable: "CabinTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_BookingCabins_BookingId",
                schema: "dbo",
                table: "BookingCabins",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingCabins_CabinTypeId",
                schema: "dbo",
                table: "BookingCabins",
                column: "CabinTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CruiseDateId",
                schema: "dbo",
                table: "Bookings",
                column: "CruiseDateId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                schema: "dbo",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CruiseDateExtras_CruiseDateId",
                schema: "dbo",
                table: "CruiseDateExtras",
                column: "CruiseDateId");

            migrationBuilder.CreateIndex(
                name: "IX_CruiseDateExtras_ExtraId",
                schema: "dbo",
                table: "CruiseDateExtras",
                column: "ExtraId");

            migrationBuilder.CreateIndex(
                name: "IX_CruiseDates_CruiseId",
                schema: "dbo",
                table: "CruiseDates",
                column: "CruiseId");

            migrationBuilder.CreateIndex(
                name: "IX_CruiseDates_ShipId",
                schema: "dbo",
                table: "CruiseDates",
                column: "ShipId");

            migrationBuilder.CreateIndex(
                name: "IX_Cruises_AllowsMinors",
                schema: "dbo",
                table: "Cruises",
                column: "AllowsMinors");

            migrationBuilder.CreateIndex(
                name: "IX_Cruises_OriginPort",
                schema: "dbo",
                table: "Cruises",
                column: "OriginPort");

            migrationBuilder.CreateIndex(
                name: "IX_Cruises_Zone",
                schema: "dbo",
                table: "Cruises",
                column: "Zone");

            migrationBuilder.CreateIndex(
                name: "IX_Extras_Name",
                schema: "dbo",
                table: "Extras",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ShipCabins_CabinTypeId",
                schema: "dbo",
                table: "ShipCabins",
                column: "CabinTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipCabins_ShipId",
                schema: "dbo",
                table: "ShipCabins",
                column: "ShipId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingCabinExtras",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CruiseDateExtras",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ShipCabins",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BookingCabins",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Extras",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Bookings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CabinTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CruiseDates",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Cruises",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Ships",
                schema: "dbo");
        }
    }
}
