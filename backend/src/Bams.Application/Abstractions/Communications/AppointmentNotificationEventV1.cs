namespace Bams.Application.Abstractions.Communications;

public sealed record AppointmentNotificationEventV1(
    Guid AppointmentId,
    string Reference,
    string Status,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
