using System.Diagnostics.CodeAnalysis;

namespace Bams.Domain;

public sealed class Appointment
{
    private readonly List<AppointmentTransition> transitions = [];

    private Appointment()
    {
    }

    [SetsRequiredMembers]
    public Appointment(
        Guid id,
        string reference,
        Guid customerId,
        Guid branchId,
        Guid serviceId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        Id = id;
        Reference = reference;
        CustomerId = customerId;
        BranchId = branchId;
        ServiceId = serviceId;
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public Guid Id
    {
        get;
        set;
    }

    public required string Reference
    {
        get;
        set;
    }

    public Guid CustomerId
    {
        get;
        set;
    }

    public Customer Customer
    {
        get;
        set;
    }
 = null!;

    public Guid BranchId
    {
        get;
        set;
    }

    public Branch Branch
    {
        get;
        set;
    }
 = null!;

    public Guid ServiceId
    {
        get;
        set;
    }

    public string? OwnerUserId
    {
        get;
        set;
    }

    public Service Service
    {
        get;
        set;
    }
 = null!;

    public DateTimeOffset StartsAt
    {
        get;
        private set;
    }

    public DateTimeOffset EndsAt
    {
        get;
        private set;
    }

    public AppointmentStatus Status
    {
        get;
        private set;
    }
 = AppointmentStatus.PendingConfirmation;

    public long Version
    {
        get;
        private set;
    }

    public IReadOnlyCollection<AppointmentTransition> Transitions => transitions.AsReadOnly();

    public AppointmentTransition Confirm()
    {
        return TransitionTo(AppointmentStatus.Confirmed, "Only a pending appointment can be confirmed.");
    }

    public AppointmentTransition Cancel()
    {
        return TransitionTo(AppointmentStatus.Cancelled, "Only a confirmed appointment can be cancelled.");
    }

    public AppointmentTransition Reschedule(DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        EnsureStatus(AppointmentStatus.Confirmed, "Only a confirmed appointment can be rescheduled.");
        if (endsAt <= startsAt)
        {
            throw new AppointmentTransitionException("An appointment must end after it starts.");
        }

        var previousStatus = Status;
        var previousStartsAt = StartsAt;
        var previousEndsAt = EndsAt;
        StartsAt = startsAt;
        EndsAt = endsAt;
        IncrementVersion();
        return RecordTransition(previousStatus, previousStartsAt, previousEndsAt);
    }

    public AppointmentTransition CheckIn()
    {
        return TransitionTo(AppointmentStatus.CheckedIn, "Only a confirmed appointment can be checked in.");
    }

    public AppointmentTransition Complete()
    {
        return TransitionTo(AppointmentStatus.Completed, "Only a checked-in appointment can be completed.");
    }

    public AppointmentTransition MarkNoShow()
    {
        return TransitionTo(AppointmentStatus.NoShow, "Only a confirmed appointment can be marked as no-show.");
    }

    private AppointmentTransition TransitionTo(AppointmentStatus nextStatus, string invalidTransitionMessage)
    {
        var allowed = (Status, nextStatus) switch
        {
            (AppointmentStatus.PendingConfirmation, AppointmentStatus.Confirmed) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.CheckedIn) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.NoShow) => true,
            (AppointmentStatus.CheckedIn, AppointmentStatus.Completed) => true,
            _ => false
        };
        if (!allowed)
        {
            throw new AppointmentTransitionException(invalidTransitionMessage);
        }

        var previousStatus = Status;
        var previousStartsAt = StartsAt;
        var previousEndsAt = EndsAt;
        Status = nextStatus;
        IncrementVersion();
        return RecordTransition(previousStatus, previousStartsAt, previousEndsAt);
    }

    private void EnsureStatus(AppointmentStatus expectedStatus, string message)
    {
        if (Status != expectedStatus)
        {
            throw new AppointmentTransitionException(message);
        }
    }

    private void IncrementVersion()
    {
        checked
        {
            Version++;
        }
    }

    private AppointmentTransition RecordTransition(
        AppointmentStatus previousStatus,
        DateTimeOffset previousStartsAt,
        DateTimeOffset previousEndsAt)
    {
        var transition = new AppointmentTransition(
            Id,
            previousStatus,
            Status,
            previousStartsAt,
            previousEndsAt,
            StartsAt,
            EndsAt,
            Version,
            DateTimeOffset.UtcNow);
        transitions.Add(transition);
        return transition;
    }
}
