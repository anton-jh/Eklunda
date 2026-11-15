using Eklunda.Web.Core.Events;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Heating.Models;
using Eklunda.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Eklunda.Web.Modules.Heating;

public class HeatingService(
    EventService eventService,
    ApplicationDbContext dbContext)
{
    public async Task CreateLogEntry(
        int? tempTankUpper,
        int? tempTankMid,
        int? tempTankLower,
        int? tempBoiler,
        bool stokedFire,
        bool litFire,
        string notes)
    {
        var userId = UserId.Example1;

        var entry = new HeatingLogEntry(DateTimeOffset.UtcNow,
            tempTankUpper, tempTankMid, tempTankLower, tempBoiler,
            litFire, stokedFire, notes);

        dbContext.HeatingLogEntries.Add(entry);

        await eventService.Publish(new CreatedHeatingLogEntryEvent(userId));
    }

    /// <summary>
    /// Entries stamped (local time) on or after <paramref name="from"/> and before
    /// <paramref name="toExclusive"/>, newest first.
    /// </summary>
    public async Task<List<HeatingLogEntry>> GetLogEntries(
        DateOnly from,
        DateOnly toExclusive,
        CancellationToken cancellationToken)
    {
        // Timestamps are stored in UTC (timestamptz); convert the local-day bounds to UTC.
        var fromUtc = StartOfLocalDayUtc(from);
        var toUtc = StartOfLocalDayUtc(toExclusive);

        return await dbContext.HeatingLogEntries
            .Where(x => x.Timestamp >= fromUtc && x.Timestamp < toUtc)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }

    private static DateTimeOffset StartOfLocalDayUtc(DateOnly date)
    {
        var local = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        return new DateTimeOffset(local).ToUniversalTime();
    }
}
