using System.Globalization;
using System.Text.RegularExpressions;
using Eklunda.Web.Modules.Heating.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Modules.Heating;

public partial class LogModel(HeatingService heatingService) : PageModel
{
    private static readonly CultureInfo Swedish = CultureInfo.GetCultureInfo("sv-SE");

    public DateOnly WeekStart { get; private set; }
    public DateOnly WeekEnd => WeekStart.AddDays(6);
    public int WeekNumber => ISOWeek.GetWeekOfYear(WeekStart.ToDateTime(TimeOnly.MinValue));
    public bool IsCurrentWeek { get; private set; }
    public string PreviousWeek => FormatWeek(WeekStart.AddDays(-7));
    public string? NextWeek => IsCurrentWeek ? null : FormatWeek(WeekStart.AddDays(7));
    public List<LogDay> Days { get; private set; } = [];

    public async Task OnGet(string? week, CancellationToken cancellationToken)
    {
        var currentWeekStart = StartOfWeek(Today);

        WeekStart = TryParseWeek(week, out var parsed) && parsed < currentWeekStart
            ? parsed
            : currentWeekStart;
        IsCurrentWeek = WeekStart == currentWeekStart;

        var entries = await heatingService.GetLogEntries(
            WeekStart, WeekStart.AddDays(7), cancellationToken);

        Days = entries
            .GroupBy(x => DateOnly.FromDateTime(x.Timestamp.LocalDateTime))
            .Select(g => new LogDay(g.Key, g.ToList()))
            .ToList();
    }

    public record LogDay(DateOnly Date, List<HeatingLogEntry> Entries);

    // --- Formatting -------------------------------------------------------------------------

    public string WeekRange
    {
        get
        {
            var (a, b) = (WeekStart, WeekEnd);
            if (a.Year != b.Year)
                return $"{a.Day} {Month(a)} {a.Year} – {b.Day} {Month(b)} {b.Year}";
            if (a.Month != b.Month)
                return $"{a.Day} {Month(a)} – {b.Day} {Month(b)} {b.Year}";
            return $"{a.Day}–{b.Day} {Month(b)} {b.Year}";
        }
    }

    public static string DayLabel(DateOnly date)
    {
        if (date == Today) return "Idag";
        if (date == Today.AddDays(-1)) return "Igår";

        var weekday = Swedish.DateTimeFormat.GetDayName(date.DayOfWeek);
        return $"{char.ToUpper(weekday[0], Swedish)}{weekday[1..]} {date.Day} {Month(date)}";
    }

    public static string Temp(int? value) => value?.ToString(Swedish) ?? "–";

    public static string Duration(TimeSpan span)
    {
        var minutes = (int)Math.Round(span.Duration().TotalMinutes);
        var (h, m) = (minutes / 60, minutes % 60);
        if (h == 0) return $"{m}min";
        return m == 0 ? $"{h}h" : $"{h}h {m}min";
    }

    private static string Month(DateOnly date)
        => Swedish.DateTimeFormat.GetAbbreviatedMonthName(date.Month).TrimEnd('.');

    // --- Week helpers -----------------------------------------------------------------------

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    private static DateOnly StartOfWeek(DateOnly date)
        => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    private static string FormatWeek(DateOnly weekStart)
    {
        var dateTime = weekStart.ToDateTime(TimeOnly.MinValue);
        return $"{ISOWeek.GetYear(dateTime)}-W{ISOWeek.GetWeekOfYear(dateTime):00}";
    }

    private static bool TryParseWeek(string? value, out DateOnly weekStart)
    {
        weekStart = default;
        var match = value is null ? null : WeekPattern().Match(value);
        if (match is not { Success: true })
            return false;

        var year = int.Parse(match.Groups[1].Value);
        var week = int.Parse(match.Groups[2].Value);
        if (year is < 2000 or > 9998 || week < 1 || week > ISOWeek.GetWeeksInYear(year))
            return false;

        weekStart = DateOnly.FromDateTime(ISOWeek.ToDateTime(year, week, DayOfWeek.Monday));
        return true;
    }

    [GeneratedRegex(@"^(\d{4})-W(\d{1,2})$", RegexOptions.IgnoreCase)]
    private static partial Regex WeekPattern();
}
