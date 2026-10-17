using System.Text.Json;
using Bams.Application.Abstractions.Communications;

namespace Bams.Infrastructure;

public sealed class SystemTextJsonMessageSerializer : IMessageSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public string Serialize<TEvent>(TEvent eventData)
    {
        return JsonSerializer.Serialize(eventData, SerializerOptions);
    }

    public TEvent Deserialize<TEvent>(string payload)
    {
        return JsonSerializer.Deserialize<TEvent>(payload, SerializerOptions)
            ?? throw new InvalidOperationException($"Could not deserialize event payload as {typeof(TEvent).Name}.");
    }
}
