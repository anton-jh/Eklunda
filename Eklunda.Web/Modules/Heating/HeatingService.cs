using Eklunda.Web.Core.Events;
using Eklunda.Web.Core.Intentions;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Heating.Models;
using Eklunda.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Eklunda.Web.Modules.Heating;

public class HeatingService(
    EventService eventService,
    IntentionService intentionService,
    ApplicationDbContext dbContext)
{
    public const string NextCheckDescription = "Kolla uppvärmningen nästa gång";

    public async Task<HeatingLogEntry> CreateLogEntry(
        UserId userId,
        int? tempTankUpper,
        int? tempTankMid,
        int? tempTankLower,
        int? tempBoiler,
        bool stokedFire,
        bool litFire,
        AmountLeftInFire? amountLeftInFire,
        string notes,
        bool takeNext)
    {
        var now = DateTimeOffset.UtcNow;

        var entry = new HeatingLogEntry(now,
            tempTankUpper, tempTankMid, tempTankLower, tempBoiler,
            litFire, stokedFire, amountLeftInFire, notes, userId);

        dbContext.HeatingLogEntries.Add(entry);

        await eventService.Publish(new CreatedHeatingLogEntryEvent(userId, now));

        // Created after the event, so it isn't closed by this very entry.
        if (takeNext)
            intentionService.Create(NextCheckDescription, userId, CreatedHeatingLogEntryEvent.Key);

        return entry;
    }

    // Returns who holds the next check afterwards.
    public async Task<UserId> TakeNext(UserId userId, CancellationToken cancellationToken)
    {
        var intention = await intentionService.TakeFirst(
            NextCheckDescription, userId, CreatedHeatingLogEntryEvent.Key, cancellationToken);

        return intention.CreatedBy;
    }

    public async Task<UserId?> GetNextCheckHolder(CancellationToken cancellationToken)
        => (await intentionService.GetOpenFor(CreatedHeatingLogEntryEvent.Key, cancellationToken))?.CreatedBy;

    /// <summary>
    /// Entries stamped (local time) on or after <paramref name="from"/> and before
    /// <paramref name="toExclusive"/>, oldest first.
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
            .OrderBy(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }

    private static DateTimeOffset StartOfLocalDayUtc(DateOnly date)
    {
        var local = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        return new DateTimeOffset(local).ToUniversalTime();
    }
}
