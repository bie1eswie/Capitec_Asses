using Bams.Application.Abstractions.Security;
using Microsoft.Extensions.Logging;

namespace Bams.Infrastructure;

public sealed class DevelopmentAccountEmailSender(
    ILogger<DevelopmentAccountEmailSender> logger) : IAccountEmailSender
{
    public Task<bool> SendPasswordResetAsync(
        string email,
        string resetUrl,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Development password reset URL for {Email}: {Url}", email, resetUrl);
        return Task.FromResult(true);
    }
}