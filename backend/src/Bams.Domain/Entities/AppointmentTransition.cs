namespace Bams.Domain;

public sealed record AppointmentTransition(
    Guid AppointmentId,
    AppointmentStatus PreviousStatus,
    AppointmentStatus CurrentStatus,
    DateTimeOffset PreviousStartsAt,
    DateTimeOffset PreviousEndsAt,
    DateTimeOffset CurrentStartsAt,
    DateTimeOffset CurrentEndsAt,
    long Version,
    DateTimeOffset OccurredAt);
