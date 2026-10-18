namespace Bams.Application.DTOs;

public enum AuthenticationStatus
{
    Succeeded,
    InvalidCredentials,
    LockedOut,
    Disabled,
    InvalidRefreshToken,
    RefreshTokenReuseDetected
}
