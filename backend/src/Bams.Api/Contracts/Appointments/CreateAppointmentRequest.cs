namespace Bams.Api.Contracts.Appointments;

public sealed class CreateAppointmentRequest
{
    public required Guid BranchId { get; init; }
    public required Guid ServiceId { get; init; }
    public required DateTimeOffset StartsAt { get; init; }
}
