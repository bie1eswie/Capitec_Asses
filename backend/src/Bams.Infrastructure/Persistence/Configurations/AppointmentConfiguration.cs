using Bams.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bams.Infrastructure;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> e)
    {
        e.ToTable("appointments");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.Reference).HasColumnName("reference");
        e.Property(x => x.CustomerId).HasColumnName("customer_id");
        e.Property(x => x.BranchId).HasColumnName("branch_id");
        e.Property(x => x.ServiceId).HasColumnName("service_id");
        e.Property(x => x.OwnerUserId).HasColumnName("owner_user_id");
        e.Property(x => x.StartsAt).HasColumnName("starts_at");
        e.Property(x => x.EndsAt).HasColumnName("ends_at");
        e.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
        e.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        e.Ignore(x => x.Transitions);
        e.HasIndex(x => x.Reference).IsUnique();
    }
}
