using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameBookingStatusAndFoldIsCanceled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Old -> new remap: PendingCheckIn(0)->PendingCheckIn(3), Completed(1)->Complete(4),
            // PendingPayment(2)->Created(1), PaymentFailed(3)->PendingPayment(2). IsCanceled=true wins
            // regardless of prior Status, folding into the new Canceled(5) value. Safe as a single
            // UPDATE despite the overlapping int ranges: Postgres evaluates the CASE per row against
            // the pre-update snapshot, so there is no cross-row collision.
            migrationBuilder.Sql("""
                UPDATE "dbo"."Bookings"
                SET "Status" = CASE
                    WHEN "IsCanceled" = true THEN 5
                    WHEN "Status" = 0 THEN 3
                    WHEN "Status" = 1 THEN 4
                    WHEN "Status" = 2 THEN 1
                    WHEN "Status" = 3 THEN 2
                    ELSE "Status"
                END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "dbo",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 2);

            migrationBuilder.DropColumn(
                name: "IsCanceled",
                schema: "dbo",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCanceled",
                schema: "dbo",
                table: "Bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE "dbo"."Bookings"
                SET "IsCanceled" = ("Status" = 5);
                """);

            // Best-effort reversal only: IsCanceled was always an independent bool, never a status
            // snapshot, so a canceled row's pre-cancellation Status was never recoverable even under
            // the old schema. Canceled(5) rows fall back to PendingCheckIn(0) as a neutral placeholder.
            migrationBuilder.Sql("""
                UPDATE "dbo"."Bookings"
                SET "Status" = CASE
                    WHEN "Status" = 5 THEN 0
                    WHEN "Status" = 4 THEN 1
                    WHEN "Status" = 1 THEN 2
                    WHEN "Status" = 2 THEN 3
                    WHEN "Status" = 3 THEN 0
                    ELSE "Status"
                END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "dbo",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 2,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);
        }
    }
}
