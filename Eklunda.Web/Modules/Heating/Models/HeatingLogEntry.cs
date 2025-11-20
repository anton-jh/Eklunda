namespace Eklunda.Web.Modules.Heating.Models;

public record HeatingLogEntry(
    DateTimeOffset Timestamp,
    int? TempTankUpper,
    int? TempTankMid,
    int? TempTankLower,
    int? TempTankBoiler,
    bool LitFire,
    bool StokedFire,
    string Notes)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
}
