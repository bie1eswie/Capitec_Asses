namespace Bams.Domain;

public sealed class BranchClosure
{
    public Guid Id
    {
        get;
        set;
    }

    public Guid BranchId
    {
        get;
        set;
    }

    public DateOnly LocalDate
    {
        get;
        set;
    }

    public string Reason
    {
        get;
        set;
    }
    = "Closed";
}
