using System;
using Bams.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bams.Infrastructure.Migrations;

[DbContext(typeof(BamsDbContext))]
[Migration("20261016000000_OutboxDeliveryMetadata")]
public partial class OutboxDeliveryMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "dead_lettered",
            table: "outbox_events",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "last_error",
            table: "outbox_events",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "locked_until",
            table: "outbox_events",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "next_attempt_at",
            table: "outbox_events",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_outbox_dispatch",
            table: "outbox_events",
            columns: new[] { "published_at", "dead_lettered", "next_attempt_at", "occurred_at" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_outbox_dispatch",
            table: "outbox_events");

        migrationBuilder.DropColumn(name: "dead_lettered", table: "outbox_events");
        migrationBuilder.DropColumn(name: "last_error", table: "outbox_events");
        migrationBuilder.DropColumn(name: "locked_until", table: "outbox_events");
        migrationBuilder.DropColumn(name: "next_attempt_at", table: "outbox_events");
    }
}
