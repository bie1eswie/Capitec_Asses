using Bams.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bams.Infrastructure;

public sealed class NotificationRecordConfiguration : IEntityTypeConfiguration<NotificationRecord>
{
    public void Configure(EntityTypeBuilder<NotificationRecord> e)
    {
        e.ToTable("notifications");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.AppointmentId).HasColumnName("appointment_id");
        e.Property(x => x.OutboxEventId).HasColumnName("outbox_event_id");
        e.HasIndex(x => x.OutboxEventId).IsUnique();
        e.Property(x => x.Channel).HasColumnName("channel");
        e.Property(x => x.SimulationStatus).HasColumnName("simulation_status");
        e.Property(x => x.RenderedContent).HasColumnName("rendered_content");
        e.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}
