using Bams.Domain;

namespace Bams.Application;

public interface IAppointmentTransitionRepository
{
    void Add(AppointmentTransition transition, string? actorId);
}
