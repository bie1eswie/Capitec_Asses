using Bams.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bams.Infrastructure.Migrations;

[DbContext(typeof(BamsDbContext))]
[Migration("20261017000000_ScopeBookingIdempotency")]
public partial class ScopeBookingIdempotency : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "operation",
            table: "booking_requests",
            type: "text",
            nullable: false,
            defaultValue: "create-appointment");

        migrationBuilder.DropIndex(
            name: "IX_booking_requests_idempotency_key",
            table: "booking_requests");

        migrationBuilder.CreateIndex(
            name: "IX_booking_requests_owner_user_id_operation_idempotency_key",
            table: "booking_requests",
            columns: new[] { "owner_user_id", "operation", "idempotency_key" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_booking_requests_owner_user_id_operation_idempotency_key",
            table: "booking_requests");

        migrationBuilder.DropColumn("operation", "booking_requests");

        migrationBuilder.CreateIndex(
            name: "IX_booking_requests_idempotency_key",
            table: "booking_requests",
            column: "idempotency_key",
            unique: true);
    }
}
