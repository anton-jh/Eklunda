using Eklunda.Web.Core.Notifications;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Pages.Settings;

public class IndexModel(
    UserService userService,
    ICurrentUser currentUser,
    VapidKeyProvider vapidKeyProvider,
    IUnitOfWork unitOfWork)
    : PageModel
{
    public string Name { get; private set; } = "";
    public string PushPublicKey { get; private set; } = "";

    [TempData]
    public string? Message { get; set; }

    public async Task OnGet(CancellationToken cancellationToken)
    {
        var user = await userService.Get(currentUser.Id, cancellationToken);
        Name = user?.Name ?? "";
        PushPublicKey = (await vapidKeyProvider.Get(cancellationToken)).PublicKey;
    }

    public async Task<IActionResult> OnPostRename(string? name, CancellationToken cancellationToken)
    {
        name = UserService.Clean(name);
        var user = await userService.Get(currentUser.Id, cancellationToken);
        if (user is not null && name != "")
        {
            user.Name = name;
            await unitOfWork.CommitAsync(cancellationToken);
            Message = "Sparat ✓";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSwitchUser()
    {
        await UserAuthentication.SignOut(HttpContext);
        return RedirectToPage("WhoAmI");
    }
}
