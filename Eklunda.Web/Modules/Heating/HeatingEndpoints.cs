using Eklunda.Web.Core.Notifications;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Shared;

namespace Eklunda.Web.Modules.Heating;

public static class HeatingEndpoints
{
    public static IEndpointRouteBuilder MapHeatingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(HeatingNotifier.TakeNextUrl, async (
            HeatingService heatingService, UserService userService, ICurrentUser currentUser,
            IUnitOfWork unitOfWork, CancellationToken cancellationToken) =>
        {
            var holder = await heatingService.TakeNext(currentUser.Id, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            if (holder == currentUser.Id)
                return new PushActionResult("Du tar nästa ✓", "Uppvärmningen");

            var names = await userService.GetNames(cancellationToken);
            var name = names.NameOf(holder);
            return new PushActionResult($"{name} hann före", "Uppvärmningen");
        });

        return app;
    }
}
