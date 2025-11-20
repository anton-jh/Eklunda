using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Modules.Heating;

public class CreateHeatingLogEntryModel(
    HeatingService heatingService)
    : PageModel
{
    public async Task OnPost(
        int? tempTankUpper,
        int? tempTankMid,
        int? tempTankLower,
        int? tempBoiler,
        bool stokedFire,
        bool litFire,
        string notes)
    {

    }
}
