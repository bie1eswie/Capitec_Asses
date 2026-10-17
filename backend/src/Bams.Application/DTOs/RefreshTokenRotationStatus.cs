namespace Bams.Application.DTOs;

public enum RefreshTokenRotationStatus
{
    Rotated,
    Invalid,
    Expired,
    ReuseDetected
}
