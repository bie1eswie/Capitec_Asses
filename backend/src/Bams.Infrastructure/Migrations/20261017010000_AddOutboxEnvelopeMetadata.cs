using Bams.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bams.Infrastructure.Migrations;

[DbContext(typeof(BamsDbContext))]
[Migration("20261017010000_AddOutboxEnvelopeMetadata")]
public partial class AddOutboxEnvelopeMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "correlation_id",
            table: "outbox_events",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "version",
            table: "outbox_events",
            type: "integer",
            nullable: false,
            defaultValue: 1);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "correlation_id", table: "outbox_events");
        migrationBuilder.DropColumn(name: "version", table: "outbox_events");
    }
}
