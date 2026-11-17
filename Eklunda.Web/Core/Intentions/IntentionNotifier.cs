using Eklunda.Web.Core.Notifications;
using Eklunda.Web.Core.Users;

namespace Eklunda.Web.Core.Intentions;

public class IntentionNotifier(
    UserService userService,
    PushNotifier pushNotifier)
{
    public async Task NotifyCreated(Intention intention, CancellationToken cancellationToken)
    {
        var names = await userService.GetNames(cancellationToken);

        await pushNotifier.SendToAllExcept(intention.CreatedBy, new PushNotification(
            Title: $"Ny avsikt från {names.NameOf(intention.CreatedBy)}",
            Body: intention.Description,
            // Unique, so intentions don't replace each other. Also the push Topic, which allows 32 characters.
            Tag: intention.Id.Value.ToString("N"),
            Url: "/Intentions"), cancellationToken);
    }
}
