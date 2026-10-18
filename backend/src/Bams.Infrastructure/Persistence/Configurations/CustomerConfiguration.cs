using Bams.Domain;
using Bams.Application.Abstractions.Security;
using Bams.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bams.Infrastructure;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> e)
    {
        e.ToTable("customers");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.FirstName).HasColumnName("first_name");
        e.Property(x => x.LastName).HasColumnName("last_name");
        e.Property(x => x.Email).HasColumnName("email");
        e.Property(x => x.IdentityUserId).HasColumnName("identity_user_id");
        e.Property(x => x.Phone).HasColumnName("phone");
        e.HasIndex(x => x.IdentityUserId).IsUnique();
        e.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.IdentityUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
