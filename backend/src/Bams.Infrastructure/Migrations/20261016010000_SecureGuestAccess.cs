using System;
using Bams.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bams.Infrastructure.Migrations;

[DbContext(typeof(BamsDbContext))]
[Migration("20261016010000_SecureGuestAccess")]
public partial class SecureGuestAccess : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "owner_user_id",
            table: "appointments",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "owner_user_id",
            table: "booking_requests",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "revoked_at",
            table: "guest_access_grants",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "permissions",
            table: "guest_access_grants",
            type: "integer",
            nullable: false,
            defaultValue: 7);

        migrationBuilder.CreateIndex(
            name: "IX_guest_grants_active",
            table: "guest_access_grants",
            columns: new[] { "appointment_id", "revoked_at", "expires_at" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_guest_grants_active", "guest_access_grants");
        migrationBuilder.DropColumn("owner_user_id", "appointments");
        migrationBuilder.DropColumn("owner_user_id", "booking_requests");
        migrationBuilder.DropColumn("revoked_at", "guest_access_grants");
        migrationBuilder.DropColumn("permissions", "guest_access_grants");
    }
}
