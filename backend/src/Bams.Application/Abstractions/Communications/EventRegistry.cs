using System.Collections.ObjectModel;

namespace Bams.Application.Abstractions.Communications;

public sealed class EventRegistry
{
    private readonly IReadOnlyDictionary<(string EventName, int Version), EventDescriptor> descriptors;

    public EventRegistry(IEnumerable<EventDescriptor> eventDescriptors)
    {
        var descriptorDictionary = new Dictionary<(string EventName, int Version), EventDescriptor>();
        foreach (var descriptor in eventDescriptors)
        {
            if (string.IsNullOrWhiteSpace(descriptor.EventName) || descriptor.Version < 1)
            {
                throw new InvalidOperationException("Event registrations require a name and positive version.");
            }

            if (!descriptorDictionary.TryAdd((descriptor.EventName, descriptor.Version), descriptor))
            {
                throw new InvalidOperationException($"Event '{descriptor.EventName}' version {descriptor.Version} is already registered.");
            }
        }

        descriptors = new ReadOnlyDictionary<(string EventName, int Version), EventDescriptor>(descriptorDictionary);
    }

    public EventDescriptor GetRequired(string eventName, int version)
    {
        if (!descriptors.TryGetValue((eventName, version), out var descriptor))
        {
            throw new InvalidOperationException($"Event '{eventName}' version {version} is not registered.");
        }

        return descriptor;
    }
}
