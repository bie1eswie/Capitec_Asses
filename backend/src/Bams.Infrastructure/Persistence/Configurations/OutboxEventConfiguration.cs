using Bams.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bams.Infrastructure;

public sealed class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> e)
    {
        e.ToTable("outbox_events");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.EventType).HasColumnName("event_type");
        e.Property(x => x.Version).HasColumnName("version").HasDefaultValue(1);
        e.Property(x => x.CorrelationId).HasColumnName("correlation_id");
        e.Property(x => x.PayloadJson).HasColumnName("payload_json").HasColumnType("jsonb");
        e.Property(x => x.OccurredAt).HasColumnName("occurred_at");
        e.Property(x => x.PublishedAt).HasColumnName("published_at");
        e.Property(x => x.Attempts).HasColumnName("attempts");
        e.Property(x => x.NextAttemptAt).HasColumnName("next_attempt_at");
        e.Property(x => x.LockedUntil).HasColumnName("locked_until");
        e.Property(x => x.LastError).HasColumnName("last_error");
        e.Property(x => x.DeadLettered).HasColumnName("dead_lettered");
        e.HasIndex(x => new { x.PublishedAt, x.DeadLettered, x.NextAttemptAt, x.OccurredAt })
            .HasDatabaseName("IX_outbox_dispatch");
    }
}
