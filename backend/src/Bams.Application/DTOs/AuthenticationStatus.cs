namespace Bams.Application.DTOs;

public enum AuthenticationStatus
{
    Succeeded,
    InvalidCredentials,
    LockedOut,
    EmailNotVerified,
    Disabled,
    InvalidRefreshToken,
    RefreshTokenReuseDetected
}
