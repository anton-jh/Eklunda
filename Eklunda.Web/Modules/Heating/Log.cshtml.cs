using System.Globalization;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Heating.Models;
using Eklunda.Web.Shared;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Modules.Heating;

public class LogModel(
    HeatingService heatingService,
    UserService userService,
    ICurrentUser currentUser)
    : PageModel
{
    public const string ToastKey = "HeatingLogToast";

    private const string DayFormat = "yyyy-MM-dd";

    public DateOnly Day { get; private set; }
    public bool IsToday => Day == Today;
    public string PreviousDay => DayParam(Day.AddDays(-1));
    public string? NextDay => IsToday ? null : DayParam(Day.AddDays(1));
    public List<HeatingLogEntry> Entries { get; private set; } = [];
    public IReadOnlyDictionary<UserId, string> Names { get; private set; } = new Dictionary<UserId, string>();
    public UserId Me => currentUser.Id;

    public string? Toast { get; private set; }

    public UserId? NextCheckHolder { get; private set; }

    public async Task OnGet(string? day, CancellationToken cancellationToken)
    {
        Toast = TempData[ToastKey] as string;
        Names = await userService.GetNames(cancellationToken);
        NextCheckHolder = await heatingService.GetNextCheckHolder(cancellationToken);

        Day = DateOnly.TryParseExact(day, DayFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
              && parsed < Today
            ? parsed
            : Today;

        Entries = await heatingService.GetLogEntries(Day, cancellationToken);
    }

    public string Who(UserId? id) => Names.NameOf(id, Me);

    public static DateOnly DayOf(HeatingLogEntry entry) => DateOnly.FromDateTime(entry.Timestamp.LocalDateTime);

    public static string DayParam(DateOnly date) => date.ToString(DayFormat, CultureInfo.InvariantCulture);

    // --- Formatting -------------------------------------------------------------------------

    public string DayName
    {
        get
        {
            if (Day == Today) return "Idag";
            if (Day == Today.AddDays(-1)) return "Igår";

            var weekday = Formats.Swedish.DateTimeFormat.GetDayName(Day.DayOfWeek);
            return $"{char.ToUpper(weekday[0], Formats.Swedish)}{weekday[1..]}";
        }
    }

    public string DayDate => Day.ToString("d MMMM yyyy", Formats.Swedish);

    public static string Duration(TimeSpan span)
    {
        var minutes = (int)Math.Round(span.Duration().TotalMinutes);
        var (h, m) = (minutes / 60, minutes % 60);
        if (h == 0) return $"{m}min";
        return m == 0 ? $"{h}h" : $"{h}h {m}min";
    }

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
}
