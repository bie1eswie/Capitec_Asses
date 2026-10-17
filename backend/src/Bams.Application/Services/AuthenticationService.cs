using System.Security.Cryptography;
using System.Text;
using Bams.Application.Abstractions.Security;
using Bams.Application.DTOs;

namespace Bams.Application.Services;

public sealed class AuthenticationService(
    IIdentityService identities,
    ITokenService tokens,
    IRefreshTokenRepository refreshTokens,
    IAccountEmailSender emailSender,
    AuthenticationServiceOptions options)
{
    public async Task<RegistrationResult> RegisterAsync(
        CustomerRegistration registration,
        CancellationToken cancellationToken)
    {
        var result = await identities.RegisterCustomerAsync(registration, cancellationToken);
        if (!result.Succeeded || string.IsNullOrWhiteSpace(result.UserId))
        {
            return new RegistrationResult(RegistrationStatus.Invalid, result.Errors);
        }

        var actionToken = await identities.CreateEmailConfirmationTokenAsync(result.UserId, cancellationToken);
        if (actionToken is null)
        {
            return new RegistrationResult(RegistrationStatus.EmailDeliveryFailed, []);
        }

        var sent = await emailSender.SendEmailConfirmationAsync(
            actionToken.Email,
            BuildActionUrl("verify-email", actionToken),
            cancellationToken);

        return new RegistrationResult(
            sent ? RegistrationStatus.Succeeded : RegistrationStatus.EmailDeliveryFailed,
            []);
    }

    public async Task<AuthenticationResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var result = await identities.AuthenticateAsync(email, password, cancellationToken);
        if (result.Status != IdentityAuthenticationStatus.Succeeded || result.Identity is null)
        {
            return new AuthenticationResult(MapIdentityStatus(result.Status), null);
        }

        var refreshToken = CreateRefreshToken();
        var now = DateTimeOffset.UtcNow;
        await refreshTokens.StoreAsync(
            result.Identity.UserId,
            Guid.NewGuid(),
            HashRefreshToken(refreshToken),
            now,
            now.AddDays(options.RefreshTokenLifetimeDays),
            cancellationToken);

        return new AuthenticationResult(
            AuthenticationStatus.Succeeded,
            CreateSession(result.Identity, refreshToken));
    }

    public async Task<AuthenticationResult> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var replacementToken = CreateRefreshToken();
        var now = DateTimeOffset.UtcNow;
        var rotation = await refreshTokens.RotateAsync(
            HashRefreshToken(refreshToken),
            HashRefreshToken(replacementToken),
            now,
            now.AddDays(options.RefreshTokenLifetimeDays),
            cancellationToken);

        if (rotation.Status != RefreshTokenRotationStatus.Rotated || string.IsNullOrWhiteSpace(rotation.UserId))
        {
            return new AuthenticationResult(MapRotationStatus(rotation.Status), null);
        }

        var identity = await identities.FindByIdAsync(rotation.UserId, cancellationToken);
        if (identity is null || !identity.IsEnabled || !identity.EmailConfirmed)
        {
            await refreshTokens.RevokeAllForUserAsync(rotation.UserId, now, cancellationToken);
            return new AuthenticationResult(
                identity?.IsEnabled == false ? AuthenticationStatus.Disabled : AuthenticationStatus.InvalidRefreshToken,
                null);
        }

        return new AuthenticationResult(
            AuthenticationStatus.Succeeded,
            CreateSession(identity, replacementToken));
    }

    public async Task LogoutAsync(string? refreshToken, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await refreshTokens.RevokeAsync(
                HashRefreshToken(refreshToken),
                DateTimeOffset.UtcNow,
                cancellationToken);
        }
    }

    public async Task LogoutAllAsync(string userId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        await refreshTokens.RevokeAllForUserAsync(userId, now, cancellationToken);
        await identities.RotateSecurityStampAsync(userId, cancellationToken);
    }

    public Task<IdentityProfile?> GetIdentityAsync(string userId, CancellationToken cancellationToken) =>
        identities.FindByIdAsync(userId, cancellationToken);

    public async Task SendEmailVerificationAsync(string email, CancellationToken cancellationToken)
    {
        var actionToken = await identities.CreateEmailConfirmationTokenForEmailAsync(email, cancellationToken);
        if (actionToken is not null)
        {
            await emailSender.SendEmailConfirmationAsync(
                actionToken.Email,
                BuildActionUrl("verify-email", actionToken),
                cancellationToken);
        }
    }

    public Task<bool> VerifyEmailAsync(
        string userId,
        string token,
        CancellationToken cancellationToken) =>
        identities.ConfirmEmailAsync(userId, token, cancellationToken);

    public async Task SendPasswordResetAsync(string email, CancellationToken cancellationToken)
    {
        var actionToken = await identities.CreatePasswordResetTokenAsync(email, cancellationToken);
        if (actionToken is not null)
        {
            await emailSender.SendPasswordResetAsync(
                actionToken.Email,
                BuildActionUrl("reset-password", actionToken),
                cancellationToken);
        }
    }

    public async Task<AccountOperationResult> ResetPasswordAsync(
        string userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var succeeded = await identities.ResetPasswordAsync(userId, token, newPassword, cancellationToken);
        if (succeeded)
        {
            await LogoutAllAsync(userId, cancellationToken);
        }

        return new AccountOperationResult(succeeded, []);
    }

    private AuthenticationSession CreateSession(IdentityProfile identity, string refreshToken) =>
        new(identity, tokens.CreateAccessToken(identity), refreshToken);

    private static AuthenticationStatus MapIdentityStatus(IdentityAuthenticationStatus status) => status switch
    {
        IdentityAuthenticationStatus.Succeeded => AuthenticationStatus.Succeeded,
        IdentityAuthenticationStatus.LockedOut => AuthenticationStatus.LockedOut,
        IdentityAuthenticationStatus.EmailNotVerified => AuthenticationStatus.EmailNotVerified,
        IdentityAuthenticationStatus.Disabled => AuthenticationStatus.Disabled,
        _ => AuthenticationStatus.InvalidCredentials
    };

    private static AuthenticationStatus MapRotationStatus(RefreshTokenRotationStatus status) => status switch
    {
        RefreshTokenRotationStatus.ReuseDetected => AuthenticationStatus.RefreshTokenReuseDetected,
        _ => AuthenticationStatus.InvalidRefreshToken
    };

    private static string CreateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string HashRefreshToken(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

    private string BuildActionUrl(string route, IdentityActionToken actionToken)
    {
        if (!Uri.TryCreate(options.ClientBaseUrl, UriKind.Absolute, out var clientUri) ||
            (!options.AllowInsecureClientUrl && clientUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("A secure account client URL is not configured.");
        }

        var baseUri = new Uri(clientUri.AbsoluteUri.TrimEnd('/') + "/");
        var actionUri = new Uri(baseUri, route);
        var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(actionToken.Token))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        var query = $"userId={Uri.EscapeDataString(actionToken.UserId)}&token={Uri.EscapeDataString(encodedToken)}";
        return new UriBuilder(actionUri) { Query = query }.Uri.ToString();
    }
}
