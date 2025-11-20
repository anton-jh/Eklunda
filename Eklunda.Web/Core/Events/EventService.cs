namespace Eklunda.Web.Core.Events;

public class EventService(IEnumerable<IEventListener> listeners)
{
    public async Task Publish(IEvent e)
    {
        foreach (var listener in listeners)
        {
            await listener.Process(e);
        }
    }
}

