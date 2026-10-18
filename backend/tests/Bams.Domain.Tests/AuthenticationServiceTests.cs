using Bams.Application;
using Bams.Application.Abstractions.Security;
using Bams.Application.DTOs;
using Bams.Application.Services;
using Xunit;

namespace Bams.Domain.Tests;

public sealed class AuthenticationServiceTests
{
    [Fact]
    public async Task RegisterAsync_CreatesAccountWithoutSendingEmail()
    {
        var service = new AuthenticationService(
            new SuccessfulIdentityService(),
            new UnusedTokenService(),
            new UnusedRefreshTokenRepository(),
            new UnusedAccountEmailSender(),
            new AuthenticationServiceOptions());

        var result = await service.RegisterAsync(
            new CustomerRegistration("customer@example.test", "Password123!", "Test", "Customer", null),
            CancellationToken.None);

        Assert.Equal(RegistrationStatus.Succeeded, result.Status);
    }

    private sealed class SuccessfulIdentityService : IIdentityService
    {
        public Task<IdentityOperationResult> RegisterCustomerAsync(
            CustomerRegistration registration,
            CancellationToken cancellationToken) =>
            Task.FromResult(new IdentityOperationResult(true, "user-1", []));

        public Task<IdentityAuthenticationResult> AuthenticateAsync(string email, string password, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<IdentityProfile?> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<IdentityActionToken?> CreatePasswordResetTokenAsync(string email, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<bool> ResetPasswordAsync(string userId, string token, string newPassword, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<bool> ValidateSecurityStampAsync(string userId, string securityStamp, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task RotateSecurityStampAsync(string userId, CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }

    private sealed class UnusedTokenService : ITokenService
    {
        public AccessToken CreateAccessToken(IdentityProfile identity) => throw new NotImplementedException();
    }

    private sealed class UnusedRefreshTokenRepository : IRefreshTokenRepository
    {
        public Task StoreAsync(string userId, Guid familyId, string tokenHash, DateTimeOffset createdAt, DateTimeOffset expiresAt, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<RefreshTokenRotation> RotateAsync(string tokenHash, string replacementTokenHash, DateTimeOffset now, DateTimeOffset replacementExpiresAt, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task RevokeAsync(string tokenHash, DateTimeOffset now, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task RevokeAllForUserAsync(string userId, DateTimeOffset now, CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }

    private sealed class UnusedAccountEmailSender : IAccountEmailSender
    {
        public Task<bool> SendPasswordResetAsync(string email, string resetUrl, CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }
}
