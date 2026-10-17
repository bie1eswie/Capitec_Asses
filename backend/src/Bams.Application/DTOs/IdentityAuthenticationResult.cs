namespace Bams.Application.DTOs;

public sealed record IdentityAuthenticationResult(
    IdentityAuthenticationStatus Status,
    IdentityProfile? Identity);
