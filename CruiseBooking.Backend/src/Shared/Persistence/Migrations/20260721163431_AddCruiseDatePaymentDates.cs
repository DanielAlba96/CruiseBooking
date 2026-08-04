using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCruiseDatePaymentDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInStartDate",
                schema: "dbo",
                table: "CruiseDates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentStartDate",
                schema: "dbo",
                table: "CruiseDates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "dbo"."CruiseDates"
                SET "PaymentStartDate" = "StartDate" - interval '30 days',
                    "CheckInStartDate" = "StartDate" - interval '15 days'
                WHERE "PaymentStartDate" IS NULL OR "CheckInStartDate" IS NULL;
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckInStartDate",
                schema: "dbo",
                table: "CruiseDates",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentStartDate",
                schema: "dbo",
                table: "CruiseDates",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInStartDate",
                schema: "dbo",
                table: "CruiseDates");

            migrationBuilder.DropColumn(
                name: "PaymentStartDate",
                schema: "dbo",
                table: "CruiseDates");
        }
    }
}
