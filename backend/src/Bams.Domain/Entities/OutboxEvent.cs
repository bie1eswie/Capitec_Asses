namespace Bams.Domain;

public sealed class OutboxEvent
{
    public Guid Id
    {
        get;
        set;
    }

    public int Version
    {
        get;
        set;
    }
    = 1;

    public string? CorrelationId
    {
        get;
        set;
    }

    public required string EventType
    {
        get;
        set;
    }

    public required string PayloadJson
    {
        get;
        set;
    }

    public DateTimeOffset OccurredAt
    {
        get;
        set;
    }

    public DateTimeOffset? PublishedAt
    {
        get;
        set;
    }

    public DateTimeOffset? NextAttemptAt
    {
        get;
        set;
    }

    public DateTimeOffset? LockedUntil
    {
        get;
        set;
    }

    public string? LastError
    {
        get;
        set;
    }

    public bool DeadLettered
    {
        get;
        set;
    }

    public int Attempts
    {
        get;
        set;
    }
}
