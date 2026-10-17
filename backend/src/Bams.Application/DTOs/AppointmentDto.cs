namespace Bams.Application;

public sealed record AppointmentDto(Guid Id, string Reference, string Status, Guid BranchId, Guid ServiceId, DateTimeOffset StartsAt, DateTimeOffset EndsAt);
