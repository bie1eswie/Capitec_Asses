using Bams.Application.Abstractions.Communications;
using Bams.Application.Abstractions.Security;
using Bams.Domain;
using Microsoft.Extensions.Logging;

namespace Bams.Application;

public sealed class AppointmentService(
    IAppointmentRepository appointments,
    IUnitOfWork unit,
    IBranchRepository branches,
    IServiceRepository services,
    IScheduleRepository schedules,
    IEventPublisher eventPublisher,
    IAppointmentTransitionRepository transitionHistory,
    ICurrentUserService currentUser,
    CreateAppointmentUseCase createAppointmentUseCase,
    Microsoft.Extensions.Logging.ILogger<AppointmentService> logger) : IAppointmentService
{
    private static AppointmentDto ToDto(Appointment a) => new(a.Id, a.Reference, a.Status.ToString(), a.BranchId, a.ServiceId, a.StartsAt, a.EndsAt);

    public async Task<AppointmentDto?> GetAsync(Guid id, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var a = await appointments.FindOwnedAsync(id, userId, ct);
        return a is null ? null : ToDto(a);
    }

    public async Task<IReadOnlyList<AppointmentDto>> ListMineAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return [];
        }

        var owned = await appointments.ListOwnedAsync(userId, ct);
        return owned.Select(ToDto).ToArray();
    }

    public async Task<IReadOnlyList<AvailableSlotDto>> AvailabilityAsync(Guid branchId, Guid serviceId, DateOnly date, CancellationToken ct)
    {
        var branch = await branches.FindActiveByIdAsync(branchId, ct);
        var service = await services.FindActiveByIdAsync(serviceId, ct);
        var bs = await branches.FindBranchServiceAsync(branchId, serviceId, ct);
        if (branch is null || service is null || bs is null || !bs.IsEnabled)
        {
            return [];
        }
        var zone = TimeZoneInfo.FindSystemTimeZoneById(branch.TimeZone);
        var startLocal = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        var utcDay = TimeZoneInfo.ConvertTimeToUtc(startLocal, zone);
        var utcNext = TimeZoneInfo.ConvertTimeToUtc(startLocal.AddDays(1), zone);
        var existing = await appointments.OverlappingAsync(branchId, serviceId, new DateTimeOffset(utcDay, TimeSpan.Zero), new DateTimeOffset(utcNext, TimeSpan.Zero), null, ct);
        var result = new List<AvailableSlotDto>();
        for (var minute = 0; minute < 24 * 60; minute += 15)
        {
            var local = startLocal.AddMinutes(minute);
            if (zone.IsInvalidTime(local) || zone.IsAmbiguousTime(local))
            {
                continue;
            }
            var utc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, zone), TimeSpan.Zero);
            var end = utc.AddMinutes(service.DurationMinutes);
            if (utc <= DateTimeOffset.UtcNow.AddMinutes(30) || utc > DateTimeOffset.UtcNow.AddDays(60))
            {
                continue;
            }
            if (!await schedules.IsOpenAsync(branchId, utc, end, branch.TimeZone, ct))
            {
                continue;
            }
            if (CapacityPolicy.HasCapacity(utc, end, existing.Select(x => (x.StartsAt, x.EndsAt)), bs.Capacity))
            {
                result.Add(new(utc, end));
            }
        }
        return result;
    }

    public async Task<BookingResultDto> BookAsync(CreateAppointmentCommand command, string key, CancellationToken ct)
    {
        return await createAppointmentUseCase.ExecuteAsync(command, key, ct);
    }

    public async Task<AppointmentDto> CancelAsync(Guid id, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new BookingException("Booking not found.", 404);
        }

        var initial = await appointments.FindOwnedAsync(id, userId, ct)
            ?? throw new BookingException("Booking not found.", 404);
        return await unit.ExecuteLockedAsync(initial.BranchId, initial.ServiceId, async token =>
        {
            var current = await appointments.FindOwnedAsync(id, userId!, token) ?? throw new BookingException("Booking not found.", 404);
            var transition = current.Cancel();
            transitionHistory.Add(transition, userId);
            await eventPublisher.PublishAsync(CreateLifecycleEnvelope(current, transition, "cancelled"), token);
            logger.LogInformation("Cancelled appointment {AppointmentId}", id);
            return ToDto(current);
        }, ct);
    }

    public async Task<AppointmentDto> RescheduleAsync(RescheduleAppointmentCommand command, CancellationToken ct)
    {
        var id = command.AppointmentId;
        var from = command.StartsAt;
        var userId = currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new BookingException("Booking not found.", 404);
        }

        var initial = await appointments.FindOwnedAsync(id, userId, ct)
            ?? throw new BookingException("Booking not found.", 404);
        return await unit.ExecuteLockedAsync(initial.BranchId, initial.ServiceId, async token =>
        {
            var current = await appointments.FindOwnedAsync(id, userId!, token) ?? throw new BookingException("Booking not found.", 404);
            var svc = await services.FindActiveByIdAsync(current.ServiceId, token);
            var branch = await branches.FindActiveByIdAsync(current.BranchId, token);
            var bs = await branches.FindBranchServiceAsync(current.BranchId, current.ServiceId, token);
            if (svc is null || branch is null || bs is null || !bs.IsEnabled)
            {
                throw new BookingException("Service unavailable.", 409);
            }

            var to = from.AddMinutes(svc.DurationMinutes);
            if (from.Offset != TimeSpan.Zero || from < DateTimeOffset.UtcNow.AddMinutes(30) || from > DateTimeOffset.UtcNow.AddDays(60) || from.Minute % 15 != 0 || from.Second != 0)
            {
                throw new BookingException("Invalid reschedule time.", 400);
            }
            if (!await schedules.IsOpenAsync(current.BranchId, from, to, branch.TimeZone, token))
            {
                throw new BookingException("Branch is closed.", 409);
            }

            var booked = await appointments.OverlappingAsync(current.BranchId, current.ServiceId, from, to, id, token);
            if (!CapacityPolicy.HasCapacity(from, to, booked.Select(a => (a.StartsAt, a.EndsAt)), bs.Capacity))
            {
                throw new BookingException("Slot unavailable.", 409);
            }

            var transition = current.Reschedule(from, to);
            transitionHistory.Add(transition, userId);
            await eventPublisher.PublishAsync(CreateLifecycleEnvelope(current, transition, "rescheduled"), token);
            logger.LogInformation("Rescheduled appointment {AppointmentId} to {StartsAt}", id, from);
            return ToDto(current);
        }, ct);
    }

    private static EventEnvelope CreateLifecycleEnvelope(
        Appointment appointment,
        AppointmentTransition transition,
        string status)
    {
        var eventData = new AppointmentNotificationEventV1(
            appointment.Id,
            appointment.Reference,
            status,
            appointment.StartsAt,
            appointment.EndsAt);
        return new EventEnvelope(
            Guid.NewGuid(),
            $"appointment.{status}",
            1,
            appointment.Id.ToString("N"),
            transition.OccurredAt,
            System.Text.Json.JsonSerializer.Serialize(eventData));
    }
}
