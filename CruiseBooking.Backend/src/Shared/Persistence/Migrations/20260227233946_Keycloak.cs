using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Keycloak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Dni",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Dni",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KeycloakId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Nationality",
                schema: "dbo",
                table: "Users",
                newName: "Country");

            migrationBuilder.AlterColumn<string>(
                name: "CardNumber",
                schema: "dbo",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "CardHolderName",
                schema: "dbo",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KeycloakUserGuid",
                schema: "dbo",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardHolderName",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KeycloakUserGuid",
                schema: "dbo",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Country",
                schema: "dbo",
                table: "Users",
                newName: "Nationality");

            migrationBuilder.AlterColumn<string>(
                name: "CardNumber",
                schema: "dbo",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dni",
                schema: "dbo",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KeycloakId",
                schema: "dbo",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Dni",
                schema: "dbo",
                table: "Users",
                column: "Dni");
        }
    }
}
