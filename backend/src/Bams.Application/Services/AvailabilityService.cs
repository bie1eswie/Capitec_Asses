using System.Linq;
using Bams.Domain;

namespace Bams.Application;

public sealed class AvailabilityService(
    IAppointmentRepository appointments,
    IScheduleRepository schedules)
{
    public async Task EnsureAvailableAsync(
        Guid branchId,
        Guid serviceId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        string timeZone,
        int capacity,
        Guid? excludedAppointmentId,
        CancellationToken cancellationToken)
    {
        var isOpen = await schedules.IsOpenAsync(
            branchId,
            startsAt,
            endsAt,
            timeZone,
            cancellationToken);
        if (!isOpen)
        {
            throw new BookingException("Branch is closed during this slot.", 409);
        }

        var overlappingAppointments = await appointments.OverlappingAsync(
            branchId,
            serviceId,
            startsAt,
            endsAt,
            excludedAppointmentId,
            cancellationToken);
        var hasCapacity = CapacityPolicy.HasCapacity(
            startsAt,
            endsAt,
            overlappingAppointments.Select(appointment => (appointment.StartsAt, appointment.EndsAt)),
            capacity);
        if (!hasCapacity)
        {
            throw new BookingException("Selected slot is no longer available.", 409);
        }
    }
}
