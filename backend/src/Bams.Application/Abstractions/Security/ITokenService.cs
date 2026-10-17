using Bams.Application.DTOs;

namespace Bams.Application.Abstractions.Security;

public interface ITokenService
{
    AccessToken CreateAccessToken(IdentityProfile identity);
}
