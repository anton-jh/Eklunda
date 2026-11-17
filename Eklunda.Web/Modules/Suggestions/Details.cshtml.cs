using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Suggestions.Models;
using Eklunda.Web.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Modules.Suggestions;

public class DetailsModel(
    SuggestionService suggestionService,
    SuggestionNotifier suggestionNotifier,
    UserService userService,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : PageModel
{
    public Suggestion Suggestion { get; private set; } = null!;
    public List<SuggestionComment> Comments { get; private set; } = [];
    public IReadOnlyDictionary<UserId, string> Names { get; private set; } = new Dictionary<UserId, string>();
    public UserId Me => currentUser.Id;
    public string? Toast { get; private set; }

    public async Task<IActionResult> OnGet(Guid id, CancellationToken cancellationToken)
    {
        var suggestion = await suggestionService.Get(id, cancellationToken);
        if (suggestion is null)
            return NotFound();

        Suggestion = suggestion;
        Toast = TempData[IndexModel.ToastKey] as string;
        Names = await userService.GetNames(cancellationToken);
        Comments = await suggestionService.GetComments(id, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostComment(Guid id, string? text, CancellationToken cancellationToken)
    {
        var suggestion = await suggestionService.Get(id, cancellationToken);
        if (suggestion is null)
            return NotFound();

        // The text input is required; this only guards against an empty post.
        if (suggestion.IsDeleted || string.IsNullOrWhiteSpace(text))
            return RedirectToPage(new { id });

        var comment = suggestionService.AddComment(suggestion, currentUser.Id, text);
        await unitOfWork.CommitAsync(cancellationToken);
        await suggestionNotifier.NotifyCommented(suggestion, comment, currentUser.Id, cancellationToken);

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostEditComment(
        Guid id, Guid commentId, string? text, CancellationToken cancellationToken)
    {
        var (suggestion, comment) = await GetOwnComment(id, commentId, cancellationToken);
        if (suggestion is null || comment is null || string.IsNullOrWhiteSpace(text) || !comment.Set(text))
            return RedirectToPage(new { id });

        await unitOfWork.CommitAsync(cancellationToken);
        await suggestionNotifier.NotifyCommentEdited(suggestion, comment, currentUser.Id, cancellationToken);

        return RedirectWithToast(id, "Sparat ✓");
    }

    public async Task<IActionResult> OnPostDeleteComment(Guid id, Guid commentId, CancellationToken cancellationToken)
    {
        var (_, comment) = await GetOwnComment(id, commentId, cancellationToken);
        if (comment is null)
            return RedirectToPage(new { id });

        suggestionService.DeleteComment(comment);
        await unitOfWork.CommitAsync(cancellationToken);

        return RedirectWithToast(id, "Borttagen ✓");
    }

    public async Task<IActionResult> OnPostRestore(Guid id, CancellationToken cancellationToken)
    {
        var suggestion = await suggestionService.Get(id, cancellationToken);
        if (suggestion is null)
            return NotFound();

        suggestion.Restore();
        await unitOfWork.CommitAsync(cancellationToken);

        return RedirectWithToast(id, "Återställt ✓");
    }

    public string Who(UserId id) => Names.NameOf(id, Me);

    // Comments on deleted suggestions are read-only.
    private async Task<(Suggestion?, SuggestionComment?)> GetOwnComment(
        Guid id, Guid commentId, CancellationToken cancellationToken)
    {
        var suggestion = await suggestionService.Get(id, cancellationToken);
        if (suggestion is null || suggestion.IsDeleted)
            return (null, null);

        return (suggestion, await suggestionService.GetOwnComment(id, commentId, Me, cancellationToken));
    }

    private RedirectToPageResult RedirectWithToast(Guid id, string toast)
    {
        TempData[IndexModel.ToastKey] = toast;
        return RedirectToPage(new { id });
    }
}
