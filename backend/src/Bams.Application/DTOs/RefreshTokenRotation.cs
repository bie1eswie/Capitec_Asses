namespace Bams.Application.DTOs;

public sealed record RefreshTokenRotation(
    RefreshTokenRotationStatus Status,
    string? UserId,
    Guid? FamilyId);
