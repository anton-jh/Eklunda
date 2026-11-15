namespace Eklunda.Web.Modules.Heating.Models;

// What the log entry form posts.
public record LogEntryValues(
    int? TempTankUpper,
    int? TempTankMid,
    int? TempTankLower,
    int? TempTankBoiler,
    bool LitFire,
    bool StokedFire,
    AmountLeftInFire? AmountLeftInFire,
    string? Notes);
