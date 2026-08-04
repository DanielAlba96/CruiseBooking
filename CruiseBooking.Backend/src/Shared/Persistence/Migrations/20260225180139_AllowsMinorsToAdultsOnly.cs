using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowsMinorsToAdultsOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AllowsMinors",
                schema: "dbo",
                table: "Cruises",
                newName: "AdultsOnly");

            migrationBuilder.RenameIndex(
                name: "IX_Cruises_AllowsMinors",
                schema: "dbo",
                table: "Cruises",
                newName: "IX_Cruises_AdultsOnly");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdultsOnly",
                schema: "dbo",
                table: "Cruises",
                newName: "AllowsMinors");

            migrationBuilder.RenameIndex(
                name: "IX_Cruises_AdultsOnly",
                schema: "dbo",
                table: "Cruises",
                newName: "IX_Cruises_AllowsMinors");
        }
    }
}
