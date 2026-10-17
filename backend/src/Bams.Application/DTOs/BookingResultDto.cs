namespace Bams.Application;

public sealed record BookingResultDto(
    Guid Id,
    string Reference,
    string Status,
    Guid BranchId,
    Guid ServiceId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    bool IsReplay);
