using Bams.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bams.Infrastructure;

public sealed class BookingRequestConfiguration : IEntityTypeConfiguration<BookingRequest>
{
    public void Configure(EntityTypeBuilder<BookingRequest> e)
    {
        e.ToTable("booking_requests");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key");
        e.Property(x => x.OwnerUserId).HasColumnName("owner_user_id");
        e.Property(x => x.Operation).HasColumnName("operation");
        e.Property(x => x.RequestHash).HasColumnName("request_hash");
        e.Property(x => x.AppointmentId).HasColumnName("appointment_id");
        e.Property(x => x.CreatedAt).HasColumnName("created_at");
        e.HasIndex(x => new { x.OwnerUserId, x.Operation, x.IdempotencyKey }).IsUnique();
    }
}
