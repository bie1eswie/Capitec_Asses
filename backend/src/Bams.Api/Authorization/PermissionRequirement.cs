using Microsoft.AspNetCore.Authorization;

namespace Bams.Api.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
