using System.Security.Cryptography;
using System.Text;
using Bams.Application;
using Bams.Application.Abstractions.Security;
using Bams.Application.DTOs;
using Bams.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bams.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> users,
    RoleManager<IdentityRole> roles,
    BamsDbContext db,
    ICustomerRepository customers) : IIdentityService
{
    public async Task<IdentityOperationResult> RegisterCustomerAsync(
        CustomerRegistration registration,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var email = registration.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            IsEnabled = true
        };

        var createResult = await users.CreateAsync(user, registration.Password);
        if (!createResult.Succeeded)
        {
            return new IdentityOperationResult(false, null, createResult.Errors.Select(x => x.Description).ToArray());
        }

        var roleResult = await EnsureRoleAsync(IdentityRoleNames.Customer);
        if (!roleResult.Succeeded)
        {
            return new IdentityOperationResult(false, null, roleResult.Errors.Select(x => x.Description).ToArray());
        }

        var addRoleResult = await users.AddToRoleAsync(user, IdentityRoleNames.Customer);
        if (!addRoleResult.Succeeded)
        {
            return new IdentityOperationResult(false, null, addRoleResult.Errors.Select(x => x.Description).ToArray());
        }

        customers.Add(new Customer
        {
            Id = Guid.NewGuid(),
            IdentityUserId = user.Id,
            FirstName = registration.FirstName.Trim(),
            LastName = registration.LastName.Trim(),
            Email = email,
            Phone = registration.Phone?.Trim()
        });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new IdentityOperationResult(true, user.Id, []);
    }

    public async Task<IdentityAuthenticationResult> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            return new IdentityAuthenticationResult(IdentityAuthenticationStatus.InvalidCredentials, null);
        }

        if (!user.IsEnabled)
        {
            return new IdentityAuthenticationResult(IdentityAuthenticationStatus.Disabled, null);
        }

        if (!user.EmailConfirmed)
        {
            return new IdentityAuthenticationResult(IdentityAuthenticationStatus.EmailNotVerified, null);
        }

        if (await users.IsLockedOutAsync(user))
        {
            return new IdentityAuthenticationResult(IdentityAuthenticationStatus.LockedOut, null);
        }

        if (!await users.CheckPasswordAsync(user, password))
        {
            await users.AccessFailedAsync(user);
            var status = await users.IsLockedOutAsync(user)
                ? IdentityAuthenticationStatus.LockedOut
                : IdentityAuthenticationStatus.InvalidCredentials;
            return new IdentityAuthenticationResult(status, null);
        }

        await users.ResetAccessFailedCountAsync(user);

        return new IdentityAuthenticationResult(
            IdentityAuthenticationStatus.Succeeded,
            await ToProfileAsync(user));
    }

    public async Task<IdentityProfile?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId);
        return user is null ? null : await ToProfileAsync(user);
    }

    public async Task<IdentityActionToken?> CreateEmailConfirmationTokenAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId);
        if (user is null || user.Email is null)
        {
            return null;
        }

        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        return new IdentityActionToken(user.Id, user.Email, token);
    }

    public async Task<IdentityActionToken?> CreateEmailConfirmationTokenForEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null || user.Email is null || user.EmailConfirmed || !user.IsEnabled)
        {
            return null;
        }

        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        return new IdentityActionToken(user.Id, user.Email, token);
    }

    public async Task<bool> ConfirmEmailAsync(
        string userId,
        string token,
        CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        var result = await users.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<IdentityActionToken?> CreatePasswordResetTokenAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null || !user.EmailConfirmed || user.Email is null || !user.IsEnabled)
        {
            return null;
        }

        var token = await users.GeneratePasswordResetTokenAsync(user);
        return new IdentityActionToken(user.Id, user.Email, token);
    }

    public async Task<bool> ResetPasswordAsync(
        string userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        var result = await users.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> ValidateSecurityStampAsync(
        string userId,
        string securityStamp,
        CancellationToken cancellationToken)
    {
        var currentStamp = await users.Users
            .AsNoTracking()
            .Where(user => user.Id == userId && user.IsEnabled && user.EmailConfirmed)
            .Select(user => user.SecurityStamp)
            .SingleOrDefaultAsync(cancellationToken);

        if (currentStamp is null)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(currentStamp),
            Encoding.UTF8.GetBytes(securityStamp));
    }

    public async Task RotateSecurityStampAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId);
        if (user is not null)
        {
            await users.UpdateSecurityStampAsync(user);
        }
    }

    private async Task<IdentityProfile> ToProfileAsync(ApplicationUser user)
    {
        var userRoles = await users.GetRolesAsync(user);
        return new IdentityProfile(
            user.Id,
            user.Email ?? string.Empty,
            userRoles.ToArray(),
            user.SecurityStamp ?? string.Empty,
            user.IsEnabled,
            user.EmailConfirmed);
    }

    private async Task<IdentityResult> EnsureRoleAsync(string roleName)
    {
        if (await roles.RoleExistsAsync(roleName))
        {
            return IdentityResult.Success;
        }

        return await roles.CreateAsync(new IdentityRole(roleName));
    }
}
