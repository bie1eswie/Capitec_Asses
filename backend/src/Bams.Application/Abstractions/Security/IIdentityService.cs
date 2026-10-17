using Bams.Application.DTOs;

namespace Bams.Application.Abstractions.Security;

public interface IIdentityService
{
    Task<IdentityOperationResult> RegisterCustomerAsync(
        CustomerRegistration registration,
        CancellationToken cancellationToken);

    Task<IdentityAuthenticationResult> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<IdentityProfile?> FindByIdAsync(string userId, CancellationToken cancellationToken);

    Task<IdentityActionToken?> CreateEmailConfirmationTokenAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<IdentityActionToken?> CreateEmailConfirmationTokenForEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<bool> ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken);

    Task<IdentityActionToken?> CreatePasswordResetTokenAsync(
        string email,
        CancellationToken cancellationToken);

    Task<bool> ResetPasswordAsync(
        string userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken);

    Task<bool> ValidateSecurityStampAsync(
        string userId,
        string securityStamp,
        CancellationToken cancellationToken);

    Task RotateSecurityStampAsync(string userId, CancellationToken cancellationToken);
}
