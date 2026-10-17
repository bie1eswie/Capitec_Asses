namespace Bams.Domain;

public sealed class NotificationRecord
{
    public Guid Id
    {
        get;
        set;
    }

    public Guid AppointmentId
    {
        get;
        set;
    }

    public Guid? OutboxEventId
    {
        get;
        set;
    }

    public required string Channel
    {
        get;
        set;
    }

    public required string SimulationStatus
    {
        get;
        set;
    }

    public required string RenderedContent
    {
        get;
        set;
    }

    public DateTimeOffset CreatedAt
    {
        get;
        set;
    }
}
