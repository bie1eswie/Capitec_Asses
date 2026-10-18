using System.Security.Claims;
using Bams.Infrastructure.Identity;

namespace Bams.Api.Authorization;

public sealed class UserPermissionResolver : IUserPermissionResolver
{
    public IReadOnlyList<string> GetGrantedPermissions(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return [];
        }

        // There is no verified staff-to-branch assignment persistence yet.
        // Do not grant branch-scoped permissions based solely on a staff role.
        if (user.IsInRole(IdentityRoleNames.SystemAdministrator))
        {
            return BamsPermissions.All;
        }

        return [];
    }
}
