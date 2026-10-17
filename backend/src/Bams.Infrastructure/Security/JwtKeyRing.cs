using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Bams.Infrastructure.Security;

public sealed class JwtKeyRing : IDisposable
{
    private readonly List<RSA> ownedKeys = [];

    public JwtKeyRing(JwtOptions options, bool isDevelopment)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer) || string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException("JWT issuer and audience must be configured.");
        }

        if (options.AccessTokenLifetimeMinutes is < 1 or > 60)
        {
            throw new InvalidOperationException("JWT access-token lifetime must be between 1 and 60 minutes.");
        }

        if (isDevelopment)
        {
            if (Encoding.UTF8.GetByteCount(options.DevelopmentSigningKey) < 32)
            {
                throw new InvalidOperationException("The development JWT signing key must be at least 32 bytes long.");
            }

            Issuer = options.Issuer;
            Audience = options.Audience;
            AccessTokenLifetime = TimeSpan.FromMinutes(options.AccessTokenLifetimeMinutes);
            ActiveKeyId = "development";
            SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.DevelopmentSigningKey))
            {
                KeyId = ActiveKeyId
            };
            ValidationKeys = [SigningKey];
            SigningAlgorithm = SecurityAlgorithms.HmacSha256;
            ValidateAudience = false;
            return;
        }

        if (options.Keys.Count == 0 || options.Keys.Any(key => string.IsNullOrWhiteSpace(key.KeyId)))
        {
            throw new InvalidOperationException("At least one JWT public signing key with a key ID must be configured.");
        }

        if (options.Keys.Select(key => key.KeyId).Distinct(StringComparer.Ordinal).Count() != options.Keys.Count)
        {
            throw new InvalidOperationException("JWT signing key IDs must be unique.");
        }

        var activeKey = options.Keys.SingleOrDefault(key => key.KeyId == options.ActiveKeyId);
        if (activeKey is null || string.IsNullOrWhiteSpace(activeKey.PrivateKeyPem))
        {
            throw new InvalidOperationException("The active JWT key must include its private key in external configuration.");
        }

        Issuer = options.Issuer;
        Audience = options.Audience;
        AccessTokenLifetime = TimeSpan.FromMinutes(options.AccessTokenLifetimeMinutes);
        ActiveKeyId = activeKey.KeyId;
        SigningKey = ImportPrivateKey(activeKey);
        ValidationKeys = options.Keys.Select(ImportPublicKey).ToArray();
        SigningAlgorithm = SecurityAlgorithms.RsaSha256;
        ValidateAudience = true;
    }

    public string Issuer { get; }

    public string Audience { get; }

    public string ActiveKeyId { get; }

    public TimeSpan AccessTokenLifetime { get; }

    public SecurityKey SigningKey { get; }

    public IReadOnlyCollection<SecurityKey> ValidationKeys { get; }

    public string SigningAlgorithm { get; }

    public bool ValidateAudience { get; }

    public SigningCredentials SigningCredentials => new(SigningKey, SigningAlgorithm);

    public TokenValidationParameters CreateValidationParameters() => new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKeys = ValidationKeys,
        ValidAlgorithms = [SigningAlgorithm],
        ValidateIssuer = true,
        ValidIssuer = Issuer,
        ValidateAudience = ValidateAudience,
        ValidAudience = Audience,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        RequireSignedTokens = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = "sub",
        RoleClaimType = "role"
    };

    public void Dispose()
    {
        foreach (var key in ownedKeys)
        {
            key.Dispose();
        }
    }

    private RsaSecurityKey ImportPrivateKey(JwtKeyOptions options)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(options.PrivateKeyPem);
        ownedKeys.Add(rsa);
        return new RsaSecurityKey(rsa) { KeyId = options.KeyId };
    }

    private SecurityKey ImportPublicKey(JwtKeyOptions options)
    {
        var rsa = RSA.Create();
        if (!string.IsNullOrWhiteSpace(options.PublicKeyPem))
        {
            rsa.ImportFromPem(options.PublicKeyPem);
        }
        else if (options.KeyId == ActiveKeyId && !string.IsNullOrWhiteSpace(options.PrivateKeyPem))
        {
            rsa.ImportParameters(((RsaSecurityKey)SigningKey).Rsa.ExportParameters(false));
        }
        else
        {
            throw new InvalidOperationException($"JWT key '{options.KeyId}' is missing a public key.");
        }

        ownedKeys.Add(rsa);
        return new RsaSecurityKey(rsa) { KeyId = options.KeyId };
    }
}
