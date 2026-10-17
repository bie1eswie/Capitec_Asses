using Bams.Application.DTOs;

namespace Bams.Application.Abstractions.Security;

public interface IRefreshTokenRepository
{
    Task StoreAsync(
        string userId,
        Guid familyId,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken);

    Task<RefreshTokenRotation> RotateAsync(
        string tokenHash,
        string replacementTokenHash,
        DateTimeOffset now,
        DateTimeOffset replacementExpiresAt,
        CancellationToken cancellationToken);

    Task RevokeAsync(string tokenHash, DateTimeOffset now, CancellationToken cancellationToken);

    Task RevokeAllForUserAsync(string userId, DateTimeOffset now, CancellationToken cancellationToken);
}
