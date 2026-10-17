namespace Bams.Domain;

public sealed class AuditEntry
{
    public Guid Id
    {
        get;
        set;
    }

    public string? DetailsJson
    {
        get;
        set;
    }

    public required string Action
    {
        get;
        set;
    }

    public required string EntityType
    {
        get;
        set;
    }

    public required string EntityId
    {
        get;
        set;
    }

    public string? ActorId
    {
        get;
        set;
    }

    public DateTimeOffset OccurredAt
    {
        get;
        set;
    }
}
