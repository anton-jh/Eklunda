using Eklunda.Web.Core.Notifications;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Heating.Models;
using Eklunda.Web.Shared;

namespace Eklunda.Web.Modules.Heating;

public class HeatingNotifier(
    HeatingService heatingService,
    UserService userService,
    PushNotifier pushNotifier)
{
    public const string TakeNextUrl = "/api/heating/take-next";

    private const int MaxNotesLength = 120;

    public Task NotifyNewEntry(HeatingLogEntry entry, CancellationToken cancellationToken)
        => Notify(entry, null, cancellationToken);

    public Task NotifyEditedEntry(HeatingLogEntry entry, UserId editedBy, CancellationToken cancellationToken)
        => Notify(entry, editedBy, cancellationToken);

    private async Task Notify(HeatingLogEntry entry, UserId? editedBy, CancellationToken cancellationToken)
    {
        var names = await userService.GetNames(cancellationToken);
        var holder = await heatingService.GetNextCheckHolder(cancellationToken);

        await pushNotifier.SendToAllExcept(
            editedBy ?? entry.CreatedBy, Build(entry, editedBy, names, holder), cancellationToken);
    }

    private static PushNotification Build(
        HeatingLogEntry entry, UserId? editedBy, IReadOnlyDictionary<UserId, string> names, UserId? holder)
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

        var title = $"{names.NameOf(entry.CreatedBy)} loggade uppvärmningen {When(entry.Timestamp)}";
        if (editedBy is not null)
            title += editedBy == entry.CreatedBy ? " (ändrad)" : $" (ändrad av {names.NameOf(editedBy)})";

        return new PushNotification(
            Title: title,
            Body: string.Join("\n", lines),
            Tag: "heating-log",
            Url: $"/Heating/Log?day={LogModel.DayParam(LogModel.DayOf(entry))}",
            Actions: holder is null ? [new PushAction("take-next", "Jag tar nästa", TakeNextUrl)] : null);
    }

    private static string When(DateTimeOffset timestamp)
    {
        var local = timestamp.ToLocalTime();
        return DateOnly.FromDateTime(local.DateTime) == DateOnly.FromDateTime(DateTime.Now)
            ? local.ToString("HH:mm")
            : local.ToString("d MMM HH:mm", Formats.Swedish).Replace(".", "");
    }
}
