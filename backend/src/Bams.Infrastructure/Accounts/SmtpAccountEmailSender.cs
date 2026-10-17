using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using Bams.Application.Abstractions.Security;
using Microsoft.Extensions.Options;

namespace Bams.Infrastructure;

public sealed class SmtpAccountEmailSender(IOptions<SmtpOptions> options) : IAccountEmailSender
{
    public Task<bool> SendEmailConfirmationAsync(
        string email,
        string confirmationUrl,
        CancellationToken cancellationToken) =>
        SendAsync(email, "Verify your BAMS account", "Verify your email address", confirmationUrl, cancellationToken);

    public Task<bool> SendPasswordResetAsync(
        string email,
        string resetUrl,
        CancellationToken cancellationToken) =>
        SendAsync(email, "Reset your BAMS password", "Reset your password", resetUrl, cancellationToken);

    private async Task<bool> SendAsync(
        string email,
        string subject,
        string action,
        string url,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.FromAddress))
        {
            return false;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(settings.FromAddress, settings.FromName),
            Subject = subject,
            Body = $"<p>{WebUtility.HtmlEncode(action)}:</p><p><a href=\"{HtmlEncoder.Default.Encode(url)}\">Continue</a></p>",
            IsBodyHtml = true
        };
        message.To.Add(email);

        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.EnableSsl
        };
        if (!string.IsNullOrWhiteSpace(settings.UserName))
        {
            client.Credentials = new NetworkCredential(settings.UserName, settings.Password);
        }

        await client.SendMailAsync(message, cancellationToken);
        return true;
    }
}
