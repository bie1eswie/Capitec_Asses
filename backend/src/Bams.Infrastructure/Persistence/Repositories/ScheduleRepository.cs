using Bams.Application;
using Microsoft.EntityFrameworkCore;

namespace Bams.Infrastructure;

public sealed class ScheduleRepository(BamsDbContext db) : IScheduleRepository
{
    public async Task<bool> IsOpenAsync(
        Guid branchId,
        DateTimeOffset start,
        DateTimeOffset end,
        string timeZone,
        CancellationToken cancellationToken)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        var localStart = TimeZoneInfo.ConvertTime(start, zone);
        var localEnd = TimeZoneInfo.ConvertTime(end, zone);
        if (localStart.Date != localEnd.Date)
        {
            return false;
        }

        var localDate = DateOnly.FromDateTime(localStart.Date);
        if (await db.BranchClosures.AnyAsync(
                closure => closure.BranchId == branchId && closure.LocalDate == localDate,
                cancellationToken))
        {
            return false;
        }

        return await db.BranchSchedules.AnyAsync(
            schedule => schedule.BranchId == branchId &&
                        schedule.IsActive &&
                        schedule.DayOfWeek == (int)localStart.DayOfWeek &&
                        schedule.OpensAt <= TimeOnly.FromDateTime(localStart.DateTime) &&
                        schedule.ClosesAt >= TimeOnly.FromDateTime(localEnd.DateTime),
            cancellationToken);
    }
}
