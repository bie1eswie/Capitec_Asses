namespace Bams.Application;

public sealed class AuthenticationServiceOptions
{
    public int RefreshTokenLifetimeDays { get; init; } = 30;

    public string? ClientBaseUrl { get; init; }

    public bool AllowInsecureClientUrl { get; init; }
}
