using Bams.Application.Abstractions.Communications;
using Bams.Domain;

namespace Bams.Application;

public sealed class OutboxFactory(IMessageSerializer serializer)
{
    public EventEnvelope CreateAppointmentConfirmed(Appointment appointment)
    {
        var eventData = new AppointmentNotificationEventV1(
                appointment.Id,
                appointment.Reference,
                appointment.Status.ToString().ToLowerInvariant(),
                appointment.StartsAt,
                appointment.EndsAt);
        return new EventEnvelope(
            Guid.NewGuid(),
            "appointment.confirmed",
            1,
            appointment.Id.ToString("N"),
            DateTimeOffset.UtcNow,
            serializer.Serialize(eventData));
    }
}
