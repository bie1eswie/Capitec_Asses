namespace Bams.Infrastructure.Security;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string ActiveKeyId { get; set; } = string.Empty;

    public string DevelopmentSigningKey { get; set; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    public List<JwtKeyOptions> Keys { get; set; } = [];
}
