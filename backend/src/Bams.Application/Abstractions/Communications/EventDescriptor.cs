namespace Bams.Application.Abstractions.Communications;

public sealed record EventDescriptor(string EventName, int Version, Type EventType);
