namespace Bams.Api.Authorization;

public static class BamsPermissions
{
    public const string AppointmentsReadBranch = "appointments.read.branch";
    public const string ServicesManage = "services.manage";
    public const string BranchSchedulesManage = "branches.schedules.manage";
    public const string StaffManage = "staff.manage";
    public const string NotificationsRead = "notifications.read";
    public const string ReportsRead = "reports.read";
    public const string OutboxReplay = "outbox.replay";

    public static readonly IReadOnlyList<string> All =
    [
        AppointmentsReadBranch,
        ServicesManage,
        BranchSchedulesManage,
        StaffManage,
        NotificationsRead,
        ReportsRead,
        OutboxReplay
    ];
}
