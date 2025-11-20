using Eklunda.Web.Core.Events;
using Eklunda.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Eklunda.Web.Core.Intentions;

public class IntentionCompletionEventListener(
    ApplicationDbContext dbContext) : IEventListener
{
    public async Task Process(IEvent e)
    {
        var eventString = e.Serialize();
        var intentions = await dbContext.Intentions.Where(x => x.DoneWhenEvent == eventString).ToListAsync();

        foreach (var intention in intentions)
        {
            intention.Done = true;
        }
    }
}
