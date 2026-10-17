using Bams.Application.Abstractions.Communications;
using Microsoft.Extensions.DependencyInjection;

namespace Bams.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddBamsApplication(
        this IServiceCollection services,
        AuthenticationServiceOptions? authenticationOptions = null)
    {
        services.AddSingleton(authenticationOptions ?? new AuthenticationServiceOptions());
        services.AddScoped<Services.AuthenticationService>();
        services.AddScoped<IBranchQueries, BranchQueries>();
        services.AddScoped<IServiceQueries, ServiceQueries>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        services.AddScoped<BookingValidator>();
        services.AddScoped<IdempotencyProvider>();
        services.AddScoped<IdempotencyService>();
        services.AddScoped<AvailabilityService>();
        services.AddScoped<CreateAppointmentUseCase>();
        services.AddScoped<AppointmentFactory>();
        services.AddScoped<OutboxFactory>();
        services.AddSingleton(new EventRegistry(
        [
            new EventDescriptor("appointment.confirmed", 1, typeof(AppointmentNotificationEventV1)),
            new EventDescriptor("appointment.cancelled", 1, typeof(AppointmentNotificationEventV1)),
            new EventDescriptor("appointment.rescheduled", 1, typeof(AppointmentNotificationEventV1))
        ]));

        return services;
    }
}
