using Eklunda.Web.Core.Users;
using Eklunda.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eklunda.Web.Pages.Settings;

[AllowAnonymous]
public class WhoAmIModel(UserService userService, IUnitOfWork unitOfWork) : PageModel
{
    public List<User> Users { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public async Task OnGet(CancellationToken cancellationToken)
    {
        Users = await userService.GetAll(cancellationToken);
    }

    public async Task<IActionResult> OnPostPick(Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.Get(new UserId(id), cancellationToken);
        if (user is null)
            return RedirectToPage(new { ReturnUrl });

        return await SignIn(user);
    }

    public async Task<IActionResult> OnPostCreate(string? name, CancellationToken cancellationToken)
    {
        if (UserService.Clean(name) is "")
            return RedirectToPage(new { ReturnUrl });

        var user = await userService.GetOrCreate(name!, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return await SignIn(user);
    }

    private async Task<IActionResult> SignIn(User user)
    {
        await UserAuthentication.SignIn(HttpContext, user);
        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/");
    }
}
