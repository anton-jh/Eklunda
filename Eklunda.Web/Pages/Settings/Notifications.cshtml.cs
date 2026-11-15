using Eklunda.Web.Core.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Pages.Settings;

public class NotificationsModel(VapidKeyProvider vapidKeyProvider) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string Next => Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/";
    public string PushPublicKey { get; private set; } = "";

    public async Task OnGet(CancellationToken cancellationToken)
    {
        PushPublicKey = (await vapidKeyProvider.Get(cancellationToken)).PublicKey;
    }
}
