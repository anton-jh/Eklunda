using System.Globalization;
using Eklunda.Web.Core.Intentions;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Pages.Intentions;

public class IndexModel(
    IntentionService intentionService,
    IntentionNotifier intentionNotifier,
    UserService userService,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : PageModel
{
    [BindProperty(SupportsGet = true)]
    public bool ShowDone { get; set; }

    public List<Intention> Open { get; private set; } = [];
    public List<Intention> Done { get; private set; } = [];
    public int DoneCount { get; private set; }
    public IReadOnlyDictionary<UserId, string> Names { get; private set; } = new Dictionary<UserId, string>();
    public UserId Me => currentUser.Id;

    public async Task OnGet(CancellationToken cancellationToken)
    {
        Names = await userService.GetNames(cancellationToken);
        Open = await intentionService.GetOpen(cancellationToken);
        DoneCount = await intentionService.CountDone(cancellationToken);
        if (ShowDone)
            Done = await intentionService.GetDone(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreate(string? description, DateTime? remindAt, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            var intention = intentionService.Create(description, currentUser.Id);
            intention.SetReminder(FromInput(remindAt));
            await unitOfWork.CommitAsync(cancellationToken);
            await intentionNotifier.NotifyCreated(intention, cancellationToken);
        }

        return RedirectToPage(new { ShowDone });
    }

    public async Task<IActionResult> OnPostDone(Guid id, CancellationToken cancellationToken)
    {
        await intentionService.MarkDone(new IntentionId(id), currentUser.Id, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return RedirectToPage(new { ShowDone });
    }

    public async Task<IActionResult> OnPostRelease(Guid id, CancellationToken cancellationToken)
    {
        await intentionService.Release(new IntentionId(id), currentUser.Id, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return RedirectToPage(new { ShowDone });
    }

    public async Task<IActionResult> OnPostRemind(Guid id, DateTime? remindAt, CancellationToken cancellationToken)
    {
        await intentionService.SetReminder(new IntentionId(id), currentUser.Id, FromInput(remindAt), cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return RedirectToPage(new { ShowDone });
    }

    public Task<IActionResult> OnPostRemoveReminder(Guid id, CancellationToken cancellationToken)
        => OnPostRemind(id, null, cancellationToken);

    public string Who(UserId id) => Names.NameOf(id, Me);

    // datetime-local inputs use the server's local time, like the rest of the app.
    public static string InputValue(DateTimeOffset? value)
        => value?.ToLocalTime().ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture) ?? "";

    private static DateTimeOffset? FromInput(DateTime? value)
        => value is null ? null : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Local));
}
