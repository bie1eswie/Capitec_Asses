using Bams.Application;
using Bams.Domain;
using Xunit;

namespace Bams.Domain.Tests;

public class OutboxFactoryTests
{
    [Fact]
    public void CreateAppointmentConfirmed_Sets_Expected_Fields()
    {
        var factory = new OutboxFactory(new TestMessageSerializer());
        var startsAt = DateTimeOffset.UtcNow;
        var appointment = new Appointment(Guid.NewGuid(), "REF", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), startsAt, startsAt.AddMinutes(15));
        var ev = factory.CreateAppointmentConfirmed(appointment);
        Assert.Equal("appointment.confirmed", ev.EventName);
        Assert.Contains(appointment.Id.ToString(), ev.Payload);
        Assert.True(ev.OccurredAt <= DateTimeOffset.UtcNow);
    }

    private sealed class TestMessageSerializer : Bams.Application.Abstractions.Communications.IMessageSerializer
    {
        public string Serialize<TEvent>(TEvent eventData) => System.Text.Json.JsonSerializer.Serialize(eventData);

        public TEvent Deserialize<TEvent>(string payload) =>
            System.Text.Json.JsonSerializer.Deserialize<TEvent>(payload)!;
    }
}
