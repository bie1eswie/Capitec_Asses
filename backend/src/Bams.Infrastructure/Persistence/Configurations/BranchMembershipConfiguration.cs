using Bams.Infrastructure.Identity;
using Bams.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bams.Infrastructure;

public sealed class BranchMembershipConfiguration : IEntityTypeConfiguration<BranchMembership>
{
    public void Configure(EntityTypeBuilder<BranchMembership> e)
    {
        e.ToTable("branch_memberships");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        e.Property(x => x.BranchId).HasColumnName("branch_id");
        e.Property(x => x.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(32);
        e.HasIndex(x => new { x.UserId, x.BranchId }).IsUnique();
        e.HasIndex(x => new { x.BranchId, x.Role });
        e.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        e.HasOne<Bams.Domain.Branch>()
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
