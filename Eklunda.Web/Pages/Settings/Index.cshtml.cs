using Eklunda.Web.Core.Notifications;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Heating;
using Eklunda.Web.Modules.Heating.Models;
using Eklunda.Web.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Pages.Settings;

public class IndexModel(
    UserService userService,
    ICurrentUser currentUser,
    VapidKeyProvider vapidKeyProvider,
    HeatingService heatingService,
    IUnitOfWork unitOfWork)
    : PageModel
{
    public string Name { get; private set; } = "";
    public string PushPublicKey { get; private set; } = "";
    public int NextCheckReminderHours { get; private set; }

    [TempData]
    public string? Message { get; set; }

    public async Task OnGet(CancellationToken cancellationToken)
    {
        var user = await userService.Get(currentUser.Id, cancellationToken);
        Name = user?.Name ?? "";
        PushPublicKey = (await vapidKeyProvider.Get(cancellationToken)).PublicKey;
        NextCheckReminderHours = (await heatingService.GetSettings(cancellationToken)).NextCheckReminderHours;
    }

    public async Task<IActionResult> OnPostHeating(int nextCheckReminderHours, CancellationToken cancellationToken)
    {
        var settings = await heatingService.GetSettings(cancellationToken);
        settings.NextCheckReminderHours = Math.Clamp(
            nextCheckReminderHours, HeatingSettings.MinReminderHours, HeatingSettings.MaxReminderHours);
        await unitOfWork.CommitAsync(cancellationToken);
        Message = "Sparat ✓";
        return RedirectToPage();
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
