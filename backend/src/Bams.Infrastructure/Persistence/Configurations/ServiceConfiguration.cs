using Bams.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bams.Infrastructure;

public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> e)
    {
        e.ToTable("services");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.Code).HasColumnName("code").HasMaxLength(40);
        e.Property(x => x.Name).HasColumnName("name").HasMaxLength(160);
        e.Property(x => x.DurationMinutes).HasColumnName("duration_minutes");
        e.Property(x => x.IsActive).HasColumnName("is_active");
        e.HasIndex(x => x.Code).IsUnique();
    }
}
