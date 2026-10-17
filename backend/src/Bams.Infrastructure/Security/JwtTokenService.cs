using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bams.Application.Abstractions.Security;
using Bams.Application.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace Bams.Infrastructure.Security;

public sealed class JwtTokenService(JwtKeyRing keyRing) : ITokenService
{
    public AccessToken CreateAccessToken(IdentityProfile identity)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.Add(keyRing.AccessTokenLifetime);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, identity.UserId),
            new(JwtRegisteredClaimNames.Email, identity.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("security_stamp", identity.SecurityStamp)
        };

        claims.AddRange(identity.Roles.Select(role => new Claim("role", role)));
        var token = new JwtSecurityToken(
            keyRing.Issuer,
            keyRing.Audience,
            claims,
            now.UtcDateTime,
            expiresAt.UtcDateTime,
            keyRing.SigningCredentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
