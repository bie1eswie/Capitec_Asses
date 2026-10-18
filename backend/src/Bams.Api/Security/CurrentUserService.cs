using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bams.Application.Abstractions.Security;

namespace Bams.Api.Security;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId => httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
}