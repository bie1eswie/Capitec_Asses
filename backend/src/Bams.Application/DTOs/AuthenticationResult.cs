namespace Bams.Application.DTOs;

public sealed record AuthenticationResult(
    AuthenticationStatus Status,
    AuthenticationSession? Session);
