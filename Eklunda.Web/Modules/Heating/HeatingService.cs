using Eklunda.Web.Core.Events;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Heating.Models;
using Eklunda.Web.Persistence;

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
}
