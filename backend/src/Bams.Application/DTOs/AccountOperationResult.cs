namespace Bams.Application.DTOs;

public sealed record AccountOperationResult(bool Succeeded, IReadOnlyList<string> Errors);
