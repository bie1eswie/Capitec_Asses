using Bams.Application.Abstractions.Communications;
using Bams.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bams.Infrastructure;

public sealed class AppointmentNotificationEventHandler(BamsDbContext db)
    : IEventHandler<AppointmentNotificationEventV1>
{
    public async Task HandleAsync(
        AppointmentNotificationEventV1 eventData,
        EventEnvelope envelope,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var alreadyProcessed = await db.Notifications
            .AnyAsync(notification => notification.OutboxEventId == envelope.EventId, cancellationToken);
        if (alreadyProcessed)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var appointment = await db.Appointments
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == eventData.AppointmentId, cancellationToken)
            ?? throw new InvalidOperationException("Appointment not found for notification event.");
        var content = $"[SIMULATED — NOT SENT] Appointment {eventData.Reference} {eventData.Status}. Current schedule: {eventData.StartsAt:yyyy-MM-dd HH:mm} UTC.";

        db.Notifications.Add(new NotificationRecord
        {
            Id = Guid.NewGuid(),
            AppointmentId = eventData.AppointmentId,
            OutboxEventId = envelope.EventId,
            Channel = "Email",
            SimulationStatus = "SIMULATED",
            RenderedContent = content,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
