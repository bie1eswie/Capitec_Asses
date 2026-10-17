using Bams.Application;
using Bams.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bams.Infrastructure;

public sealed class AppointmentRepository(BamsDbContext db) : IAppointmentRepository
{
    public Task<Appointment?> FindAsync(Guid id, CancellationToken ct) => db.Appointments.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Appointment?> FindOwnedAsync(Guid id, string identityUserId, CancellationToken ct) =>
        db.Appointments.FirstOrDefaultAsync(
            appointment => appointment.Id == id && appointment.Customer.IdentityUserId == identityUserId,
            ct);

    public async Task<IReadOnlyList<Appointment>> ListOwnedAsync(string identityUserId, CancellationToken ct) =>
        await db.Appointments.AsNoTracking()
            .Where(appointment => appointment.Customer.IdentityUserId == identityUserId)
            .OrderByDescending(appointment => appointment.StartsAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Appointment>> OverlappingAsync(Guid branchId, Guid serviceId, DateTimeOffset from, DateTimeOffset to, Guid? exclude, CancellationToken ct) => await db.Appointments.AsNoTracking().Where(a => a.BranchId == branchId && a.ServiceId == serviceId && (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.CheckedIn) && a.StartsAt < to && a.EndsAt > from && (!exclude.HasValue || a.Id != exclude.Value)).ToListAsync(ct);

    public void Add(Appointment appointment) => db.Appointments.Add(appointment);
}
