namespace Bams.Domain;

public sealed class Customer
{
    public Guid Id
    {
        get;
        set;
    }

    public required string FirstName
    {
        get;
        set;
    }

    public required string LastName
    {
        get;
        set;
    }

    public required string Email
    {
        get;
        set;
    }

    public string? IdentityUserId
    {
        get;
        set;
    }

    public string? Phone
    {
        get;
        set;
    }
}
