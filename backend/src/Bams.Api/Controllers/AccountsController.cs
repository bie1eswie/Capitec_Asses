using Bams.Api.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
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
    IAntiforgery antiforgery,
    IConfiguration configuration,
    IUserPermissionResolver permissions) : ControllerBase
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
            RegistrationStatus.Succeeded => Accepted(new { message = "Your account is ready. You can now sign in." }),
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
            AuthenticationStatus.Succeeded when result.Session is not null => CreateSessionResponse(result.Session),
            AuthenticationStatus.LockedOut => StatusCode(StatusCodes.Status429TooManyRequests),
            AuthenticationStatus.Disabled => Forbid(),
            _ => Unauthorized()
        };
    }

    [AllowAnonymous]
    [EnableRateLimiting("account")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(HttpContext);

        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized();
        }

        var result = await authentication.RefreshAsync(refreshToken, cancellationToken);
        if (result.Status != AuthenticationStatus.Succeeded || result.Session is null)
        {
            DeleteRefreshCookie();
            return Unauthorized();
        }

        return CreateSessionResponse(result.Session);
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
    public IActionResult Session()
    {
        return Ok(new
        {
            UserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub),
            Email = User.FindFirstValue(JwtRegisteredClaimNames.Email),
            Roles = User.FindAll("role").Select(claim => claim.Value).ToArray(),
            Permissions = permissions.GetGrantedPermissions(User)
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(HttpContext);
        Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken);
        await authentication.LogoutAsync(refreshToken, cancellationToken);
        DeleteRefreshCookie();
        return NoContent();
    }

    private const string RefreshCookieName = "Bams.Refresh";

    private IActionResult CreateSessionResponse(AuthenticationSession session)
    {
        Response.Cookies.Append(
            RefreshCookieName,
            session.RefreshToken,
            GetRefreshCookieOptions());
        Response.Headers.CacheControl = "no-store";

        return Ok(new
        {
            userId = session.Identity.UserId,
            email = session.Identity.Email,
            roles = session.Identity.Roles,
            permissions = permissions.GetGrantedPermissions(
                CreatePermissionPrincipal(session.Identity.Roles)),
            accessToken = session.AccessToken.Value,
            accessTokenExpiresAt = session.AccessToken.ExpiresAt
        });
    }

    private static System.Security.Claims.ClaimsPrincipal CreatePermissionPrincipal(
        IReadOnlyList<string> roles)
    {
        var identity = new System.Security.Claims.ClaimsIdentity(
            roles.Select(role => new System.Security.Claims.Claim("role", role)),
            "Identity",
            System.Security.Claims.ClaimTypes.Name,
            "role");

        return new System.Security.Claims.ClaimsPrincipal(identity);
    }

    private void DeleteRefreshCookie()
    {
        Response.Cookies.Delete(RefreshCookieName, GetRefreshCookieOptions());
    }

    private CookieOptions GetRefreshCookieOptions()
    {
        var days = configuration.GetValue<int?>("Authentication:RefreshTokenLifetimeDays") ?? 30;
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/v1/accounts",
            Expires = DateTimeOffset.UtcNow.AddDays(days),
            IsEssential = true
        };
    }

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
