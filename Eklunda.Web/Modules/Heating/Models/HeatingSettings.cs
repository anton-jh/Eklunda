namespace Eklunda.Web.Modules.Heating.Models;

// Shared by everyone. A single row.
public class HeatingSettings
{
    public const int MinReminderHours = 1;
    public const int MaxReminderHours = 24;

    public int Id { get; private set; } = 1;
    public int NextCheckReminderHours { get; set; } = 2;
}
