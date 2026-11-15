using Eklunda.Web.Core.Notifications;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Heating.Models;

namespace Eklunda.Web.Modules.Heating;

public class HeatingNotifier(
    HeatingService heatingService,
    UserService userService,
    PushNotifier pushNotifier)
{
    public const string TakeNextUrl = "/api/heating/take-next";

    private const int MaxNotesLength = 120;

    public async Task NotifyNewEntry(HeatingLogEntry entry, CancellationToken cancellationToken)
    {
        var names = await userService.GetNames(cancellationToken);
        var holder = await heatingService.GetNextCheckHolder(cancellationToken);

        await pushNotifier.SendToAllExcept(entry.CreatedBy, Build(entry, names, holder), cancellationToken);
    }

    private static PushNotification Build(HeatingLogEntry entry, IReadOnlyDictionary<UserId, string> names, UserId? holder)
    {
        var lines = new List<string>();

        var summary = HeatingFormat.Summary(entry);
        if (summary != "")
            lines.Add(summary);

        if (!string.IsNullOrWhiteSpace(entry.Notes))
        {
            var notes = entry.Notes.Trim();
            lines.Add(notes.Length > MaxNotesLength ? notes[..MaxNotesLength] + "…" : notes);
        }

        lines.Add(holder is null ? "Ingen har tagit nästa än." : $"{names.NameOf(holder)} tar nästa.");

        return new PushNotification(
            Title: $"{names.NameOf(entry.CreatedBy)} loggade uppvärmningen {entry.Timestamp.ToLocalTime():HH:mm}",
            Body: string.Join("\n", lines),
            Tag: "heating-log",
            Url: "/Heating/Log",
            Actions: holder is null ? [new PushAction("take-next", "Jag tar nästa", TakeNextUrl)] : null);
    }
}
