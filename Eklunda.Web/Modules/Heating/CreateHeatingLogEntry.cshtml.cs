using Eklunda.Web.Shared;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Modules.Heating;

public class CreateHeatingLogEntryModel(
    HeatingService heatingService,
    IUnitOfWork unitOfWork)
    : PageModel
{
    public async Task OnPost(
        int? tempTankUpper,
        int? tempTankMid,
        int? tempTankLower,
        int? tempBoiler,
        bool stokedFire,
        bool litFire,
        string notes,
        CancellationToken cancellationToken)
    {
        await heatingService.CreateLogEntry(
            tempTankUpper, tempTankMid, tempTankLower, tempBoiler,
            stokedFire, litFire, notes);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
