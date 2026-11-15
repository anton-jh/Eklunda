using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Heating.Models;
using Eklunda.Web.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Modules.Heating;

public class CreateLogEntryModel(
    HeatingService heatingService,
    HeatingNotifier heatingNotifier,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(
        int? tempTankUpper,
        int? tempTankMid,
        int? tempTankLower,
        int? tempBoiler,
        bool stokedFire,
        bool litFire,
        AmountLeftInFire? amountLeftInFire,
        string? notes,
        bool? takeNext,
        CancellationToken cancellationToken)
    {
        if (takeNext is not { } answer)
            return BadRequest();

        var entry = await heatingService.CreateLogEntry(
            currentUser.Id,
            tempTankUpper, tempTankMid, tempTankLower, tempBoiler,
            stokedFire, litFire, amountLeftInFire, notes?.Trim() ?? "",
            answer);

        await unitOfWork.CommitAsync(cancellationToken);
        await heatingNotifier.NotifyNewEntry(entry, cancellationToken);

        TempData[LogModel.SavedKey] = true;
        return RedirectToPage("Log");
    }
}
