using System.Security.Claims;
using Bams.Api.Contracts.Accounts;
using Bams.Application;
using Bams.Application.DTOs;
using Bams.Application.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Bams.Api.Controllers;

[ApiController]
[Route("api/v1/accounts")]
public sealed class AccountsController(
    AuthenticationService authentication,
    IAntiforgery antiforgery) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("csrf-token")]
    public IActionResult GetCsrfToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        Response.Headers.CacheControl = "no-store";
        return Ok(new { requestToken = tokens.RequestToken });
    }

    [AllowAnonymous]
    [EnableRateLimiting("account")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authentication.RegisterAsync(
            new CustomerRegistration(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Phone),
            cancellationToken);

        return result.Status switch
        {
            RegistrationStatus.Succeeded => Accepted(new { message = "Check your email to verify your account." }),
            RegistrationStatus.EmailDeliveryFailed => StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Title = "Email delivery is not configured.",
                Status = StatusCodes.Status503ServiceUnavailable
            }),
            _ => BadRequest(result.Errors)
        };
    }

    [AllowAnonymous]
    [EnableRateLimiting("account")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authentication.LoginAsync(request.Email, request.Password, cancellationToken);
        return result.Status switch
        {
            AuthenticationStatus.Succeeded when result.Session is not null => Ok(ToSessionResponse(result.Session)),
            AuthenticationStatus.LockedOut => StatusCode(StatusCodes.Status429TooManyRequests),
            AuthenticationStatus.EmailNotVerified or AuthenticationStatus.Disabled => Forbid(),
            _ => Unauthorized()
        };
    }

    [AllowAnonymous]
    [EnableRateLimiting("account")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest();
        }

        var result = await authentication.RefreshAsync(request.RefreshToken, cancellationToken);
        return result.Status == AuthenticationStatus.Succeeded && result.Session is not null
            ? Ok(ToSessionResponse(result.Session))
            : Unauthorized();
    }

    [AllowAnonymous]
    [EnableRateLimiting("account")]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] EmailTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) || !TryDecodeToken(request.Token, out var token))
        {
            return BadRequest();
        }

        return await authentication.VerifyEmailAsync(request.UserId, token, cancellationToken)
            ? NoContent()
            : BadRequest();
    }

    [AllowAnonymous]
    [EnableRateLimiting("account")]
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(
        [FromBody] PasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            await authentication.SendEmailVerificationAsync(request.Email.Trim(), cancellationToken);
        }

        return Accepted(new { message = "If the account needs verification, an email will be sent." });
    }

    [AllowAnonymous]
    [EnableRateLimiting("account")]
    [HttpPost("password/forgot")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] PasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            await authentication.SendPasswordResetAsync(request.Email.Trim(), cancellationToken);
        }

        return Accepted(new { message = "If the account exists, a password reset email will be sent." });
    }

    [AllowAnonymous]
    [EnableRateLimiting("account")]
    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] PasswordResetConfirmationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) ||
            string.IsNullOrWhiteSpace(request.NewPassword) ||
            !TryDecodeToken(request.Token, out var token))
        {
            return BadRequest();
        }

        var result = await authentication.ResetPasswordAsync(
            request.UserId,
            token,
            request.NewPassword,
            cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Errors);
    }

    [Authorize]
    [HttpGet("session")]
    public IActionResult Session() => Ok(new
    {
        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
        Email = User.FindFirstValue(ClaimTypes.Email)
    });

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequest? request,
        CancellationToken cancellationToken)
    {
        await authentication.LogoutAsync(request?.RefreshToken, cancellationToken);
        return NoContent();
    }

    private static object ToSessionResponse(AuthenticationSession session) => new
    {
        userId = session.Identity.UserId,
        email = session.Identity.Email,
        roles = session.Identity.Roles,
        accessToken = session.AccessToken.Value,
        accessTokenExpiresAt = session.AccessToken.ExpiresAt,
        refreshToken = session.RefreshToken
    };

    private static bool TryDecodeToken(string? token, out string decodedToken)
    {
        try
        {
            decodedToken = System.Text.Encoding.UTF8.GetString(
                Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(token ?? string.Empty));
            return !string.IsNullOrWhiteSpace(decodedToken);
        }
        catch (FormatException)
        {
            decodedToken = string.Empty;
            return false;
        }
    }
}
