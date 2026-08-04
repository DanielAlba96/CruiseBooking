using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLockedCabins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LockedCabins",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CruiseDateId = table.Column<int>(type: "integer", nullable: false),
                    CabinId = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockedCabins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LockedCabins_CruiseDates_CruiseDateId",
                        column: x => x.CruiseDateId,
                        principalSchema: "dbo",
                        principalTable: "CruiseDates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LockedCabins_ShipCabins_CabinId",
                        column: x => x.CabinId,
                        principalSchema: "dbo",
                        principalTable: "ShipCabins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LockedCabins_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LockedCabins_CabinId",
                schema: "dbo",
                table: "LockedCabins",
                column: "CabinId");

            migrationBuilder.CreateIndex(
                name: "IX_LockedCabins_CruiseDateId",
                schema: "dbo",
                table: "LockedCabins",
                column: "CruiseDateId");

            migrationBuilder.CreateIndex(
                name: "IX_LockedCabins_UserId",
                schema: "dbo",
                table: "LockedCabins",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LockedCabins",
                schema: "dbo");
        }
    }
}
