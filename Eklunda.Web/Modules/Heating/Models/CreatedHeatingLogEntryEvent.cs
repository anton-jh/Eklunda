using Eklunda.Web.Core.Events;
using Eklunda.Web.Core.Users;

namespace Eklunda.Web.Modules.Heating.Models;

public class CreatedHeatingLogEntryEvent(UserId userId)
    : IEvent
{
    public UserId UserId { get; } = userId;


    public string Serialize()
        => $"{nameof(CreatedHeatingLogEntryEvent)}_{UserId}";
}
