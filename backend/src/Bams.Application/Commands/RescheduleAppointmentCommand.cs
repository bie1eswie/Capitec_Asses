namespace Bams.Application;

public sealed record RescheduleAppointmentCommand(
    Guid AppointmentId,
    DateTimeOffset StartsAt);
