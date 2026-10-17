namespace Bams.Application.DTOs;

public enum IdentityAuthenticationStatus
{
    Succeeded,
    InvalidCredentials,
    LockedOut,
    EmailNotVerified,
    Disabled
}
