using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Suggestions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Modules.Suggestions;

public class IndexModel(
    SuggestionService suggestionService,
    UserService userService,
    ICurrentUser currentUser)
    : PageModel
{
    public const string ToastKey = "SuggestionsToast";

    [BindProperty(SupportsGet = true)]
    public bool ShowDeleted { get; set; }

    public List<Suggestion> Suggestions { get; private set; } = [];
    public List<Suggestion> Deleted { get; private set; } = [];
    public int DeletedCount { get; private set; }
    public IReadOnlyDictionary<Guid, int> CommentCounts { get; private set; } = new Dictionary<Guid, int>();
    public IReadOnlyDictionary<UserId, string> Names { get; private set; } = new Dictionary<UserId, string>();
    public string? Toast { get; private set; }

    public async Task OnGet(CancellationToken cancellationToken)
    {
        Toast = TempData[ToastKey] as string;
        Names = await userService.GetNames(cancellationToken);
        Suggestions = await suggestionService.GetActive(cancellationToken);
        CommentCounts = await suggestionService.GetCommentCounts(cancellationToken);
        DeletedCount = await suggestionService.CountDeleted(cancellationToken);
        if (ShowDeleted)
            Deleted = await suggestionService.GetDeleted(cancellationToken);
    }

    public string Who(UserId id) => Names.NameOf(id, currentUser.Id);

    public string Comments(Suggestion suggestion)
        => CommentCounts.GetValueOrDefault(suggestion.Id) switch
        {
            0 => "",
            1 => " · 1 kommentar",
            var count => $" · {count} kommentarer",
        };
}
