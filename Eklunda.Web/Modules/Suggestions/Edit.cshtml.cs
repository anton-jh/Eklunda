using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Suggestions.Models;
using Eklunda.Web.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Modules.Suggestions;

// New suggestion without an id, edit with one.
public class EditModel(
    SuggestionService suggestionService,
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
        return Suggestion is null ? NotFound() : Page();
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
            suggestionService.Create(currentUser.Id, title, description);
        }
        else
        {
            var suggestion = await suggestionService.Get(id.Value, cancellationToken);
            if (suggestion is null)
                return NotFound();

            suggestion.Set(title, description);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return RedirectToIndex("Sparat ✓");
    }

    public async Task<IActionResult> OnPostDelete(Guid id, CancellationToken cancellationToken)
    {
        var suggestion = await suggestionService.Get(id, cancellationToken);
        if (suggestion is null)
            return RedirectToPage("Index");

        suggestionService.Delete(suggestion);
        await unitOfWork.CommitAsync(cancellationToken);

        return RedirectToIndex("Borttaget ✓");
    }

    private RedirectToPageResult RedirectToIndex(string toast)
    {
        TempData[IndexModel.ToastKey] = toast;
        return RedirectToPage("Index");
    }
}
