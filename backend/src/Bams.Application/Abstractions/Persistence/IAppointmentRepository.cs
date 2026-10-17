using Bams.Domain;

namespace Bams.Application;

public interface IAppointmentRepository
{
    Task<Appointment?> FindAsync(Guid id, CancellationToken cancellationToken);
    Task<Appointment?> FindOwnedAsync(Guid id, string identityUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Appointment>> ListOwnedAsync(string identityUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Appointment>> OverlappingAsync(Guid branchId, Guid serviceId, DateTimeOffset from, DateTimeOffset to, Guid? excludingId, CancellationToken cancellationToken);
    void Add(Appointment appointment);
}
