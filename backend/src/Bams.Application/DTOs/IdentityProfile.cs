namespace Bams.Application.DTOs;

public sealed record IdentityProfile(
    string UserId,
    string Email,
    IReadOnlyList<string> Roles,
    string SecurityStamp,
    bool IsEnabled,
    bool EmailConfirmed);
