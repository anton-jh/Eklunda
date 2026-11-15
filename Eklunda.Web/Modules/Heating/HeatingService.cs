using System.Globalization;
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

        var entry = new HeatingLogEntry(DateTimeOffset.Now,
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
        // SQLite stores DateTimeOffset as TEXT ("yyyy-MM-dd HH:mm:ss.FFFFFFFzzz"), and EF can't
        // translate comparisons or ordering on it. Entries are stamped with local time, so a plain
        // text comparison against date prefixes filters by local calendar day.
        var fromText = from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var toText = toExclusive.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var entries = await dbContext.HeatingLogEntries
            .FromSql($"SELECT * FROM HeatingLogEntries WHERE Timestamp >= {fromText} AND Timestamp < {toText}")
            .ToListAsync(cancellationToken);

        return entries.OrderByDescending(x => x.Timestamp).ToList();
    }
}
