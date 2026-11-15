using Eklunda.Web.Modules.Heating.Models;
using Eklunda.Web.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Modules.Heating;

public class CreateLogEntryModel(
    HeatingService heatingService,
    IUnitOfWork unitOfWork)
    : PageModel
{
    [TempData]
    public bool Saved { get; set; }

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
        CancellationToken cancellationToken)
    {
        await heatingService.CreateLogEntry(
            tempTankUpper, tempTankMid, tempTankLower, tempBoiler,
            stokedFire, litFire, amountLeftInFire, notes?.Trim() ?? "");

        await unitOfWork.CommitAsync(cancellationToken);

        Saved = true;
        return RedirectToPage("Log");
    }
}
