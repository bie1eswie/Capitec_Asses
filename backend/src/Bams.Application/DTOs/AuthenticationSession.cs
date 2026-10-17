namespace Bams.Application.DTOs;

public sealed record AuthenticationSession(
    IdentityProfile Identity,
    AccessToken AccessToken,
    string RefreshToken);
