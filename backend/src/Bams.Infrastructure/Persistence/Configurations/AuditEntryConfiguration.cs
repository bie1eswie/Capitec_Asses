using Bams.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bams.Infrastructure;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> e)
    {
        e.ToTable("audit_entries");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.Action).HasColumnName("action");
        e.Property(x => x.EntityType).HasColumnName("entity_type");
        e.Property(x => x.EntityId).HasColumnName("entity_id");
        e.Property(x => x.ActorId).HasColumnName("actor_id");
        e.Property(x => x.DetailsJson).HasColumnName("details_json");
        e.Property(x => x.OccurredAt).HasColumnName("occurred_at");
    }
}
