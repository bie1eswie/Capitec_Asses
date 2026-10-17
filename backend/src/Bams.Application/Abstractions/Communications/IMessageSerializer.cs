namespace Bams.Application.Abstractions.Communications;

public interface IMessageSerializer
{
    string Serialize<TEvent>(TEvent eventData);

    TEvent Deserialize<TEvent>(string payload);
}
