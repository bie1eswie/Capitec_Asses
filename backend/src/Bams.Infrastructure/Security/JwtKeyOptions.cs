namespace Bams.Infrastructure.Security;

public sealed class JwtKeyOptions
{
    public string KeyId { get; set; } = string.Empty;

    public string PublicKeyPem { get; set; } = string.Empty;

    public string? PrivateKeyPem { get; set; }
}
