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
            Tag: Tag(intention),
            Url: "/Intentions"), cancellationToken);
    }

    // Each reminder replaces the previous one. Dismissing or tapping it stops the reminders.
    public Task NotifyReminder(Intention intention, CancellationToken cancellationToken)
        => pushNotifier.SendTo(intention.CreatedBy, new PushNotification(
            Title: "Påminnelse",
            Body: intention.Description,
            Tag: Tag(intention),
            Url: "/Intentions",
            Actions:
            [
                new PushAction("release", "Släpp", IntentionEndpoints.ReleaseUrl(intention.Id)),
                new PushAction("done", "Klar", IntentionEndpoints.DoneUrl(intention.Id)),
            ],
            DismissUrl: IntentionEndpoints.StopRemindingUrl(intention.Id)), cancellationToken);

    // Unique, so intentions don't replace each other. Also the push Topic, which allows 32 characters.
    private static string Tag(Intention intention) => intention.Id.Value.ToString("N");
}
