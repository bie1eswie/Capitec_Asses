namespace Bams.Domain;

public sealed class BookingRequest
{
    public Guid Id
    {
        get;
        set;
    }

    public required string Operation
    {
        get;
        set;
    }

    public string? OwnerUserId
    {
        get;
        set;
    }

    public required string IdempotencyKey
    {
        get;
        set;
    }

    public required string RequestHash
    {
        get;
        set;
    }

    public Guid AppointmentId
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
