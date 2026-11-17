using System.Globalization;

namespace Eklunda.Web.Shared;

public static class Formats
{
    public static readonly CultureInfo Swedish = CultureInfo.GetCultureInfo("sv-SE");

    // "idag 14:05", "igår 09:30", "3 nov".
    public static string When(DateTimeOffset value) => Format(value, alwaysTime: false);

    // Like When, but always with the time: "3 nov 14:05".
    public static string WhenWithTime(DateTimeOffset value) => Format(value, alwaysTime: true);

    private static string Format(DateTimeOffset value, bool alwaysTime)
    {
        var local = value.ToLocalTime();
        var date = DateOnly.FromDateTime(local.DateTime);
        var today = DateOnly.FromDateTime(DateTime.Now);
        var time = local.ToString("HH:mm");

        if (date == today) return $"idag {time}";
        if (date == today.AddDays(-1)) return $"igår {time}";
        if (date == today.AddDays(1)) return $"imorgon {time}";

        var day = local.ToString(date.Year == today.Year ? "d MMM" : "d MMM yyyy", Swedish).Replace(".", "");
        return alwaysTime ? $"{day} {time}" : day;
    }
}
