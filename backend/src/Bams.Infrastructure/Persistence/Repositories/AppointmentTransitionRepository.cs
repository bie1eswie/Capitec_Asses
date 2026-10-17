using Bams.Application;
using Bams.Domain;
using System.Text.Json;

namespace Bams.Infrastructure;

public sealed class AppointmentTransitionRepository(BamsDbContext db) : IAppointmentTransitionRepository
{
    public void Add(AppointmentTransition transition, string? actorId)
    {
        db.AuditEntries.Add(new AuditEntry
        {
            Id = Guid.NewGuid(),
            Action = $"appointment.{transition.CurrentStatus.ToString().ToLowerInvariant()}",
            EntityType = nameof(Appointment),
            EntityId = transition.AppointmentId.ToString(),
            ActorId = actorId,
            DetailsJson = JsonSerializer.Serialize(transition),
            OccurredAt = transition.OccurredAt
        });
    }
}
