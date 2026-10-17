namespace Bams.Application.Abstractions.Communications;

public sealed record AppointmentEventV1(Guid AppointmentId, string? Reference);
