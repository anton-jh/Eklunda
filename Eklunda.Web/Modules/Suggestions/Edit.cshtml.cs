using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Suggestions.Models;
using Eklunda.Web.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Modules.Suggestions;

// New suggestion without an id, edit with one.
public class EditModel(
    SuggestionService suggestionService,
    SuggestionNotifier suggestionNotifier,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : PageModel
{
    public Suggestion? Suggestion { get; private set; }

    public async Task<IActionResult> OnGet(Guid? id, CancellationToken cancellationToken)
    {
        if (id is null)
            return Page();

        Suggestion = await suggestionService.Get(id.Value, cancellationToken);
        if (Suggestion is null)
            return NotFound();

        return Suggestion.IsDeleted ? RedirectToPage("Details", new { id }) : Page();
    }

    public async Task<IActionResult> OnPost(
        Guid? id,
        string? title,
        string? description,
        CancellationToken cancellationToken)
    {
        // The title input is required; this only guards against an empty post.
        if (string.IsNullOrWhiteSpace(title))
            return RedirectToPage(new { id });

        if (id is null)
        {
            var created = suggestionService.Create(currentUser.Id, title, description);
            await unitOfWork.CommitAsync(cancellationToken);
            await suggestionNotifier.NotifyCreated(created, currentUser.Id, cancellationToken);
            return RedirectWithToast("Index", null, "Sparat ✓");
        }

        var suggestion = await suggestionService.Get(id.Value, cancellationToken);
        if (suggestion is null)
            return NotFound();

        if (!suggestion.IsDeleted && suggestion.Set(title, description))
        {
            await unitOfWork.CommitAsync(cancellationToken);
            await suggestionNotifier.NotifyEdited(suggestion, currentUser.Id, cancellationToken);
        }

        return RedirectWithToast("Details", id, "Sparat ✓");
    }

    public async Task<IActionResult> OnPostDelete(Guid id, CancellationToken cancellationToken)
    {
        var suggestion = await suggestionService.Get(id, cancellationToken);
        if (suggestion is null)
            return RedirectToPage("Index");

        suggestion.Delete(currentUser.Id);
        await unitOfWork.CommitAsync(cancellationToken);

        return RedirectWithToast("Index", null, "Borttaget ✓");
    }

    private RedirectToPageResult RedirectWithToast(string page, Guid? id, string toast)
    {
        TempData[IndexModel.ToastKey] = toast;
        return RedirectToPage(page, new { id });
    }
}
