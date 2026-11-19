using Eklunda.Web.Core.Users;

namespace Eklunda.Web.Modules.Heating.Models;

public class HeatingLogEntry(DateTimeOffset timestamp, UserId? createdBy)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; private set; } = timestamp;
    public int? TempTankUpper { get; private set; }
    public int? TempTankMid { get; private set; }
    public int? TempTankLower { get; private set; }
    public int? TempTankBoiler { get; private set; }
    public int? Shunt { get; private set; }
    public bool LitFire { get; private set; }
    public bool StokedFire { get; private set; }
    public StokeAmount? StokeAmount { get; private set; }
    public bool SweptBoiler { get; private set; }
    public AmountLeftInFire? AmountLeftInFire { get; private set; }
    public FireState? FireState { get; private set; }
    public string Notes { get; private set; } = "";
    public UserId? CreatedBy { get; private set; } = createdBy;

    public void Set(LogEntryValues values)
    {
        TempTankUpper = values.TempTankUpper;
        TempTankMid = values.TempTankMid;
        TempTankLower = values.TempTankLower;
        TempTankBoiler = values.TempTankBoiler;
        Shunt = values.Shunt;
        LitFire = values.LitFire;
        StokedFire = values.StokedFire;
        // The form hides the amount but still posts it.
        StokeAmount = values.StokedFire ? values.StokeAmount : null;
        SweptBoiler = values.SweptBoiler;
        AmountLeftInFire = values.AmountLeftInFire;
        FireState = values.FireState;
        Notes = values.Notes?.Trim() ?? "";
    }
}
