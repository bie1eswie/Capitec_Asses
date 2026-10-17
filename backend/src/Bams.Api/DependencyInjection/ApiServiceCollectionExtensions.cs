using Bams.Application;
using Bams.Application.Abstractions.Security;
using Bams.Infrastructure;
using Bams.Infrastructure.Identity;
using Bams.Infrastructure.Security;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;

namespace Bams.Api.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddBamsApi(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        services.AddProblemDetails(options =>
        {
            options.Map<BookingException>((_, exception) => new ProblemDetails
            {
                Type = "about:blank",
                Title = "Booking error",
                Status = exception.StatusCode,
                Detail = exception.Message
            });
            options.Map<ValidationException>((_, exception) =>
            {
                var problemDetails = new ValidationProblemDetails();
                foreach (var failure in exception.Errors)
                {
                    var property = failure.PropertyName ?? string.Empty;
                    if (!problemDetails.Errors.ContainsKey(property))
                    {
                        problemDetails.Errors[property] = [failure.ErrorMessage];
                    }
                    else
                    {
                        problemDetails.Errors[property] = problemDetails.Errors[property]
                            .Concat([failure.ErrorMessage])
                            .ToArray();
                    }
                }

                problemDetails.Type = "about:blank";
                problemDetails.Title = "Validation error";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                return problemDetails;
            });
            options.IncludeExceptionDetails = (_, _) => environment.IsDevelopment();
        });

        services.AddControllers();
        services.AddHttpContextAccessor();
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Type = "about:blank",
                    Title = "Validation error",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "One or more validation errors occurred. See the errors property for details."
                };
                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json", "application/problem+xml" }
                };
            };
        });
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter a JWT access token using the Bearer scheme.",
                In = Microsoft.OpenApi.ParameterLocation.Header,
                Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            });
            options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
            {
                [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });
        services.AddOpenApi();

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequiredLength = 12;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<BamsDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
            options.ClaimsIdentity.UserIdClaimType = System.Security.Claims.ClaimTypes.NameIdentifier);

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<JwtKeyRing>((options, keyRing) =>
            {
                options.MapInboundClaims = false;
                options.SaveToken = false;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.TokenValidationParameters = keyRing.CreateValidationParameters();
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userId = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                        var securityStamp = context.Principal?.FindFirst("security_stamp")?.Value;
                        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(securityStamp))
                        {
                            context.Fail("The access token is missing required identity claims.");
                            return;
                        }

                        var identityService = context.HttpContext.RequestServices.GetRequiredService<IIdentityService>();
                        if (!await identityService.ValidateSecurityStampAsync(
                                userId,
                                securityStamp,
                                context.HttpContext.RequestAborted))
                        {
                            context.Fail("The access token is no longer valid.");
                        }
                    }
                };
            });

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__Host-Bams.Csrf";
            options.Cookie.Path = "/";
            options.Cookie.Domain = null;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.AddPolicy("Customer", policy => policy.RequireRole(IdentityRoleNames.Customer));
            options.AddPolicy("SystemAdministrator", policy => policy.RequireRole(IdentityRoleNames.SystemAdministrator));
        });

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (allowedOrigins.Any(origin => origin == "*"))
        {
            throw new InvalidOperationException("Credentialed CORS cannot allow wildcard origins.");
        }

        services.AddCors(options => options.AddPolicy("bams-spa", policy =>
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins);
            }

            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }));
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("account", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
        });

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHealthChecks().AddDbContextCheck<BamsDbContext>("postgres");

        return services;
    }
}
