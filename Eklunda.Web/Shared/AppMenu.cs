namespace Eklunda.Web.Shared;

/// <summary>
/// The app's navigation, shown on the start page and in the hamburger menu.
/// Each module gets its own group; add new modules here.
/// </summary>
public static class AppMenu
{
    public static IReadOnlyList<MenuGroup> Groups { get; } =
    [
        new("Uppvärmning",
        [
            new("Ny post", "/Modules/Heating/CreateLogEntry"),
            new("Logg", "/Modules/Heating/Log"),
        ]),
    ];
}

public record MenuGroup(string Title, IReadOnlyList<MenuItem> Items);

/// <param name="Page">Razor page path, as used by <c>asp-page</c>.</param>
public record MenuItem(string Title, string Page);
