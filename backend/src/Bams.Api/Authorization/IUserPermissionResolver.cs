using System.Security.Claims;

namespace Bams.Api.Authorization;

public interface IUserPermissionResolver
{
    IReadOnlyList<string> GetGrantedPermissions(ClaimsPrincipal user);
}
