using Bams.Domain;

namespace Bams.Application;

public sealed class AppointmentFactory
{
    public Appointment Create(Customer customer, CreateAppointmentCommand command, int durationMinutes)
    {
        var appointment = new Appointment(
            Guid.NewGuid(),
            "BAMS-" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            customer.Id,
            command.BranchId,
            command.ServiceId,
            command.StartsAt,
            command.StartsAt.AddMinutes(durationMinutes))
        {
            Customer = customer
        };
        appointment.Confirm();

        return appointment;
    }
}
