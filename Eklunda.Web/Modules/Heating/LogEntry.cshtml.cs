using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Heating.Models;
using Eklunda.Web.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Modules.Heating;

// New entry without an id, edit with one.
public class LogEntryModel(
    HeatingService heatingService,
    HeatingNotifier heatingNotifier,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : PageModel
{
    public HeatingLogEntry? Entry { get; private set; }

    public async Task<IActionResult> OnGet(Guid? id, CancellationToken cancellationToken)
    {
        if (id is null)
            return Page();

        Entry = await heatingService.GetLogEntry(id.Value, cancellationToken);
        return Entry is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPost(
        Guid? id,
        LogEntryValues values,
        CancellationToken cancellationToken)
    {
        HeatingLogEntry? entry;
        if (id is null)
        {
            entry = await heatingService.CreateLogEntry(currentUser.Id, values);
            await unitOfWork.CommitAsync(cancellationToken);
            await heatingNotifier.NotifyNewEntry(entry, cancellationToken);
        }
        else
        {
            entry = await heatingService.GetLogEntry(id.Value, cancellationToken);
            if (entry is null)
                return NotFound();

            entry.Set(values);
            await unitOfWork.CommitAsync(cancellationToken);
            await heatingNotifier.NotifyEditedEntry(entry, currentUser.Id, cancellationToken);
        }

        return RedirectToLog(entry, "Sparat ✓");
    }

    public async Task<IActionResult> OnPostDelete(Guid id, CancellationToken cancellationToken)
    {
        var entry = await heatingService.GetLogEntry(id, cancellationToken);
        if (entry is null)
            return RedirectToPage("Log");

        heatingService.DeleteLogEntry(entry);
        await unitOfWork.CommitAsync(cancellationToken);

        return RedirectToLog(entry, "Borttagen ✓");
    }

    private RedirectToPageResult RedirectToLog(HeatingLogEntry entry, string toast)
    {
        TempData[LogModel.ToastKey] = toast;
        return RedirectToPage("Log", new { day = LogModel.DayParam(LogModel.DayOf(entry)) });
    }
}
