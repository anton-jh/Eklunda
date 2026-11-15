using Eklunda.Web.Modules.Heating.Models;
using Eklunda.Web.Shared;

namespace Eklunda.Web.Modules.Heating;

public static class HeatingFormat
{
    public static string Temp(int? value) => value?.ToString(Formats.Swedish) ?? "–";

    public static string AmountLeft(AmountLeftInFire amount) => amount switch
    {
        AmountLeftInFire.None => "inget",
        AmountLeftInFire.Some => "lite",
        AmountLeftInFire.Much => "mycket",
        _ => amount.ToString()
    };

    public static bool HasTank(HeatingLogEntry entry)
        => entry.TempTankUpper is not null || entry.TempTankMid is not null || entry.TempTankLower is not null;

    public static string Summary(HeatingLogEntry entry)
    {
        var parts = new List<string>();
        if (HasTank(entry))
            parts.Add($"Tank {Temp(entry.TempTankUpper)} · {Temp(entry.TempTankMid)} · {Temp(entry.TempTankLower)}°");
        if (entry.TempTankBoiler is { } boiler)
            parts.Add($"Panna {boiler}°");
        if (entry.AmountLeftInFire is { } amountLeft)
            parts.Add($"Kvar {AmountLeft(amountLeft)}");
        if (entry.LitFire)
            parts.Add("Tänt");
        if (entry.StokedFire)
            parts.Add("Lagt på");
        return string.Join(" · ", parts);
    }
}
