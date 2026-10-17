namespace Bams.Application;

public sealed class CreateAppointmentCommand
{
    public required Guid BranchId { get; init; }
    public required Guid ServiceId { get; init; }
    public required DateTimeOffset StartsAt { get; init; }
}
