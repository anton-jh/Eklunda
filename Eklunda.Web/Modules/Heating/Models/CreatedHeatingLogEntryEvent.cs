using Eklunda.Web.Core.Events;
using Eklunda.Web.Core.Users;

namespace Eklunda.Web.Modules.Heating.Models;

// The key leaves out the user, so any new entry closes the open "next check" intention.
public class CreatedHeatingLogEntryEvent(UserId userId, DateTimeOffset occurredAt)
    : IEvent
{
    public const string Key = nameof(CreatedHeatingLogEntryEvent);

    public DateTimeOffset OccurredAt { get; } = occurredAt;
    public UserId? TriggeredBy { get; } = userId;

    public string Serialize() => Key;
}
