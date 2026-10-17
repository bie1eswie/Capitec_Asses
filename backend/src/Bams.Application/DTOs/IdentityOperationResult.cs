namespace Bams.Application.DTOs;

public sealed record IdentityOperationResult(
    bool Succeeded,
    string? UserId,
    IReadOnlyList<string> Errors);
