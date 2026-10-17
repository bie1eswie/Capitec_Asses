using Bams.Application.Abstractions.Security;
using Bams.Application.DTOs;
using Bams.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bams.Infrastructure;

public sealed class RefreshTokenRepository(BamsDbContext db) : IRefreshTokenRepository
{
    public async Task StoreAsync(
        string userId,
        Guid familyId,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        db.RefreshSessions.Add(new RefreshSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = familyId,
            TokenHash = tokenHash,
            CreatedAt = createdAt,
            ExpiresAt = expiresAt
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshTokenRotation> RotateAsync(
        string tokenHash,
        string replacementTokenHash,
        DateTimeOffset now,
        DateTimeOffset replacementExpiresAt,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var session = await db.RefreshSessions
            .FromSqlInterpolated($"SELECT * FROM refresh_sessions WHERE token_hash = {tokenHash} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        if (session is null)
        {
            return new RefreshTokenRotation(RefreshTokenRotationStatus.Invalid, null, null);
        }

        if (session.RevokedAt.HasValue)
        {
            await db.RefreshSessions
                .Where(x => x.FamilyId == session.FamilyId && x.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, now), cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new RefreshTokenRotation(RefreshTokenRotationStatus.ReuseDetected, session.UserId, session.FamilyId);
        }

        if (session.ExpiresAt <= now)
        {
            session.RevokedAt = now;
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new RefreshTokenRotation(RefreshTokenRotationStatus.Expired, session.UserId, session.FamilyId);
        }

        var replacement = new RefreshSession
        {
            Id = Guid.NewGuid(),
            UserId = session.UserId,
            FamilyId = session.FamilyId,
            TokenHash = replacementTokenHash,
            CreatedAt = now,
            ExpiresAt = replacementExpiresAt
        };
        session.RevokedAt = now;
        session.ReplacedBySessionId = replacement.Id;
        db.RefreshSessions.Add(replacement);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new RefreshTokenRotation(RefreshTokenRotationStatus.Rotated, session.UserId, session.FamilyId);
    }

    public async Task RevokeAsync(string tokenHash, DateTimeOffset now, CancellationToken cancellationToken)
    {
        await db.RefreshSessions
            .Where(x => x.TokenHash == tokenHash && x.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, now), cancellationToken);
    }

    public async Task RevokeAllForUserAsync(
        string userId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await db.RefreshSessions
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, now), cancellationToken);
    }
}
