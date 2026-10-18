using Microsoft.AspNetCore.Authorization;

namespace Bams.Api.Authorization;

public sealed class PermissionAuthorizationHandler(
    IUserPermissionResolver resolver) : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (resolver.GetGrantedPermissions(context.User).Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
