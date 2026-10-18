using Bams.Application;
using Bams.Application.Abstractions.Communications;
using Bams.Application.Abstractions.Security;
using Bams.Infrastructure.Identity;
using Bams.Infrastructure.Seeding;
using Bams.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bams.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddBamsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var jwtKeyRing = new JwtKeyRing(jwtOptions, isDevelopment);
        services.AddSingleton(jwtOptions);
        services.AddSingleton(jwtKeyRing);

        services.AddDbContext<BamsDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres")));
        services.AddScoped<BamsSeedService>();

        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAppointmentTransitionRepository, AppointmentTransitionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IBookingRequestRepository, BookingRequestRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IBranchMembershipRepository, BranchMembershipRepository>();

        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        var smtpConfigured = !string.IsNullOrWhiteSpace(configuration[$"{SmtpOptions.SectionName}:Host"])
            && !string.IsNullOrWhiteSpace(configuration[$"{SmtpOptions.SectionName}:FromAddress"]);
        if (isDevelopment && !smtpConfigured)
        {
            services.AddScoped<IAccountEmailSender, DevelopmentAccountEmailSender>();
        }
        else
        {
            services.AddScoped<IAccountEmailSender, SmtpAccountEmailSender>();
        }

        services.AddScoped<IMessageSerializer, SystemTextJsonMessageSerializer>();
        services.AddScoped<IEventPublisher, PostgreSqlEventPublisher>();
        services.AddScoped<IEventSubscriber, PostgreSqlEventSubscriber>();
        services.AddScoped<IOutboxDispatcher, PostgreSqlEventWorkerDispatcher>();
        services.AddScoped<IEventHandler<AppointmentNotificationEventV1>, AppointmentNotificationEventHandler>();
        services.AddScoped<IEventHandlerInvoker>(serviceProvider => new EventHandlerInvoker<AppointmentNotificationEventV1>(
            "appointment.confirmed", 1,
            serviceProvider.GetRequiredService<IMessageSerializer>(),
            serviceProvider.GetServices<IEventHandler<AppointmentNotificationEventV1>>()));
        services.AddScoped<IEventHandlerInvoker>(serviceProvider => new EventHandlerInvoker<AppointmentNotificationEventV1>(
            "appointment.cancelled", 1,
            serviceProvider.GetRequiredService<IMessageSerializer>(),
            serviceProvider.GetServices<IEventHandler<AppointmentNotificationEventV1>>()));
        services.AddScoped<IEventHandlerInvoker>(serviceProvider => new EventHandlerInvoker<AppointmentNotificationEventV1>(
            "appointment.rescheduled", 1,
            serviceProvider.GetRequiredService<IMessageSerializer>(),
            serviceProvider.GetServices<IEventHandler<AppointmentNotificationEventV1>>()));

        services.AddOptions<MessagingOptions>()
            .Bind(configuration.GetSection(MessagingOptions.SectionName))
            .Validate(options => options.PollIntervalSeconds > 0 && options.BatchSize > 0 && options.MaximumAttempts > 0 && options.LeaseDurationSeconds > 0, "Messaging polling, batch, retry, and lease values must be positive.")
            .ValidateOnStart();
        services.AddHostedService<PostgreSqlEventWorker>();

        return services;
    }
}
