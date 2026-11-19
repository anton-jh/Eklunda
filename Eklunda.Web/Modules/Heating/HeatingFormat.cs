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

    public static string Fire(FireState state) => state switch
    {
        FireState.None => "Ingen eld",
        FireState.Embers => "Glöd",
        FireState.Flames => "Eld",
        _ => state.ToString()
    };

    public static string Stoked(StokeAmount? amount) => amount switch
    {
        StokeAmount.Little => "Lagt på lite",
        StokeAmount.Half => "Lagt på halv",
        StokeAmount.Full => "Lagt på full",
        _ => "Lagt på"
    };

    public static bool HasTank(HeatingLogEntry entry)
        => entry.TempTankUpper is not null || entry.TempTankMid is not null || entry.TempTankLower is not null;

    public static string Summary(HeatingLogEntry entry)
    {
        var parts = new List<string>();
        if (entry.TempTankBoiler is { } boiler)
            parts.Add($"Panna {boiler}°");
        if (HasTank(entry))
            parts.Add($"Tank {Temp(entry.TempTankUpper)} · {Temp(entry.TempTankMid)} · {Temp(entry.TempTankLower)}°");
        if (entry.Shunt is { } shunt)
            parts.Add($"Shunt {shunt}");
        if (entry.AmountLeftInFire is { } amountLeft)
            parts.Add($"Kvar {AmountLeft(amountLeft)}");
        if (entry.FireState is { } fire)
            parts.Add(Fire(fire));
        if (entry.LitFire)
            parts.Add("Tänt");
        if (entry.StokedFire)
            parts.Add(Stoked(entry.StokeAmount));
        if (entry.SweptBoiler)
            parts.Add("Sotat");
        return string.Join(" · ", parts);
    }
}
