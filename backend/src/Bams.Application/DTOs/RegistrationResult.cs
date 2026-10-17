namespace Bams.Application.DTOs;

public sealed record RegistrationResult(
    RegistrationStatus Status,
    IReadOnlyList<string> Errors);
