namespace Bams.Application;

public interface IAppointmentService
{
    Task<IReadOnlyList<AvailableSlotDto>> AvailabilityAsync(Guid branchId, Guid serviceId, DateOnly localDate, CancellationToken ct);
    Task<BookingResultDto> BookAsync(CreateAppointmentCommand command, string idempotencyKey, CancellationToken ct);
    Task<AppointmentDto?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<AppointmentDto>> ListMineAsync(CancellationToken ct);
    Task<AppointmentDto> CancelAsync(Guid id, CancellationToken ct);
    Task<AppointmentDto> RescheduleAsync(RescheduleAppointmentCommand command, CancellationToken ct);
}
