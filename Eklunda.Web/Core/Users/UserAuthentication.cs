using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Eklunda.Web.Core.Users;

// No passwords: a long-lived cookie holds the chosen user's id.
public static class UserAuthentication
{
    public const string WhoAmIPath = "/Settings/WhoAmI";

    public static Task SignIn(HttpContext httpContext, User user)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, user.Id.Value.ToString())],
            CookieAuthenticationDefaults.AuthenticationScheme);

        return httpContext.SignInAsync(
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }

    public static Task SignOut(HttpContext httpContext) => httpContext.SignOutAsync();

    public static UserId? GetUserId(ClaimsPrincipal? principal)
        => Guid.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? new UserId(id) : null;
}
