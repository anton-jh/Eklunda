using Eklunda.Web.Core.Users;

namespace Eklunda.Web.Core.Events;

public interface IEvent
{
    string Serialize();

    DateTimeOffset OccurredAt { get; }

    UserId? TriggeredBy { get; }
}
