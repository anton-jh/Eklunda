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

    public async Task<IActionResult> OnPostCreate(string? description, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            var intention = intentionService.Create(description, currentUser.Id);
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

    public string Who(UserId id) => Names.NameOf(id, Me);

    public static string When(DateTimeOffset value)
    {
        var local = value.ToLocalTime();
        var date = DateOnly.FromDateTime(local.DateTime);
        var today = DateOnly.FromDateTime(DateTime.Now);
        var time = local.ToString("HH:mm");

        if (date == today) return $"idag {time}";
        if (date == today.AddDays(-1)) return $"igår {time}";
        return local.ToString(date.Year == today.Year ? "d MMM" : "d MMM yyyy", Formats.Swedish).Replace(".", "");
    }
}
