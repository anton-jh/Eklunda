using Eklunda.Web.Core.Notifications;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Suggestions.Models;
using Eklunda.Web.Shared;

namespace Eklunda.Web.Modules.Suggestions;

public class SuggestionNotifier(
    UserService userService,
    PushNotifier pushNotifier)
{
    private const int MaxBodyLength = 200;

    public Task NotifyCreated(Suggestion suggestion, UserId by, CancellationToken cancellationToken)
        => Notify(suggestion, by, name => $"Nytt förslag från {name}", SuggestionBody(suggestion), cancellationToken);

    public Task NotifyEdited(Suggestion suggestion, UserId by, CancellationToken cancellationToken)
        => Notify(suggestion, by, name => $"{name} ändrade ett förslag", SuggestionBody(suggestion), cancellationToken);

    public Task NotifyCommented(Suggestion suggestion, SuggestionComment comment, UserId by, CancellationToken cancellationToken)
        => Notify(suggestion, by, name => $"{name} kommenterade ”{suggestion.Title}”", CommentBody(comment), cancellationToken);

    public Task NotifyCommentEdited(Suggestion suggestion, SuggestionComment comment, UserId by, CancellationToken cancellationToken)
        => Notify(suggestion, by, name => $"{name} ändrade en kommentar på ”{suggestion.Title}”", CommentBody(comment), cancellationToken);

    // One tag per suggestion, so new notifications about it replace older ones.
    private async Task Notify(
        Suggestion suggestion, UserId by, Func<string, string> title, string body, CancellationToken cancellationToken)
    {
        var names = await userService.GetNames(cancellationToken);

        await pushNotifier.SendToAllExcept(by, new PushNotification(
            Title: title(names.NameOf(by)),
            Body: body,
            Tag: suggestion.Id.ToString("N"),
            Url: $"/Suggestions/Details/{suggestion.Id}"), cancellationToken);
    }

    private static string SuggestionBody(Suggestion suggestion)
        => suggestion.Description.Length > 0
            ? $"{suggestion.Title}\n{Strings.Shorten(suggestion.Description, MaxBodyLength)}"
            : suggestion.Title;

    private static string CommentBody(SuggestionComment comment)
        => Strings.Shorten(comment.Text, MaxBodyLength);
}
