using Microsoft.AspNetCore.Identity;

namespace Bams.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public bool IsEnabled { get; set; } = true;
}
