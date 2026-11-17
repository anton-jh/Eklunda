namespace Eklunda.Web.Shared;

public static class Strings
{
    // Trimmed and cut to fit, for storing user input.
    public static string Clean(string? value, int maxLength)
    {
        value = (value ?? "").Trim();
        return value.Length > maxLength ? value[..maxLength] : value;
    }

    // Trimmed and cut with an ellipsis, for display.
    public static string Shorten(string value, int maxLength)
    {
        value = value.Trim();
        return value.Length > maxLength ? value[..maxLength] + "…" : value;
    }
}
