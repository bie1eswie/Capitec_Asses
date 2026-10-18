namespace Bams.Application.Abstractions.Security;

public interface IAccountEmailSender
{
    Task<bool> SendPasswordResetAsync(string email, string resetUrl, CancellationToken cancellationToken);
}
