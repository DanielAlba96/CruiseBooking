using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Country",
                schema: "dbo",
                table: "Users",
                newName: "Address_Country");

            migrationBuilder.RenameColumn(
                name: "Address",
                schema: "dbo",
                table: "Users",
                newName: "Address_Street");

            migrationBuilder.AddColumn<string>(
                name: "Address_Locality",
                schema: "dbo",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_PostalCode",
                schema: "dbo",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_Region",
                schema: "dbo",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_Locality",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Address_PostalCode",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Address_Region",
                schema: "dbo",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Address_Country",
                schema: "dbo",
                table: "Users",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "Address_Street",
                schema: "dbo",
                table: "Users",
                newName: "Address");
        }
    }
}
