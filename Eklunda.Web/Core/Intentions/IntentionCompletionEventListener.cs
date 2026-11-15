using Eklunda.Web.Core.Events;

namespace Eklunda.Web.Core.Intentions;

public class IntentionCompletionEventListener(
    IntentionService intentionService) : IEventListener
{
    public Task Process(IEvent e)
        => intentionService.MarkDoneFor(e.Serialize(), e.OccurredAt, e.TriggeredBy, CancellationToken.None);
}
