using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Suggestions.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Modules.Suggestions;

public class IndexModel(
    SuggestionService suggestionService,
    UserService userService,
    ICurrentUser currentUser)
    : PageModel
{
    public const string ToastKey = "SuggestionsToast";

    public List<Suggestion> Suggestions { get; private set; } = [];
    public IReadOnlyDictionary<UserId, string> Names { get; private set; } = new Dictionary<UserId, string>();
    public string? Toast { get; private set; }

    public async Task OnGet(CancellationToken cancellationToken)
    {
        Toast = TempData[ToastKey] as string;
        Names = await userService.GetNames(cancellationToken);
        Suggestions = await suggestionService.GetAll(cancellationToken);
    }

    public string Who(UserId id) => Names.NameOf(id, currentUser.Id);
}
