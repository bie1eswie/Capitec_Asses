using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Bams.Application.Abstractions.Security;

namespace Bams.Api;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId => httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
