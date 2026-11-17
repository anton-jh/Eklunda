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

    public async Task<HeatingLogEntry> CreateLogEntry(UserId userId, LogEntryValues values)
    {
        var now = DateTimeOffset.UtcNow;

        var entry = new HeatingLogEntry(now, userId);
        entry.Set(values);

        dbContext.HeatingLogEntries.Add(entry);

        await eventService.Publish(new CreatedHeatingLogEntryEvent(userId, now));

        return entry;
    }

    public Task<(Intention Intention, bool Created)> TakeNext(UserId userId, CancellationToken cancellationToken)
        => intentionService.TakeFirst(NextCheckDescription, userId, CreatedHeatingLogEntryEvent.Key, cancellationToken);

    public async Task<UserId?> GetNextCheckHolder(CancellationToken cancellationToken)
        => (await intentionService.GetOpenFor(CreatedHeatingLogEntryEvent.Key, cancellationToken))?.CreatedBy;

    public Task<HeatingLogEntry?> GetLogEntry(Guid id, CancellationToken cancellationToken)
        => dbContext.HeatingLogEntries.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void DeleteLogEntry(HeatingLogEntry entry)
        => dbContext.HeatingLogEntries.Remove(entry);

    // Entries stamped (local time) on the given day, oldest first.
    public async Task<List<HeatingLogEntry>> GetLogEntries(DateOnly day, CancellationToken cancellationToken)
    {
        // Timestamps are stored in UTC (timestamptz); convert the local-day bounds to UTC.
        var fromUtc = StartOfLocalDayUtc(day);
        var toUtc = StartOfLocalDayUtc(day.AddDays(1));

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
