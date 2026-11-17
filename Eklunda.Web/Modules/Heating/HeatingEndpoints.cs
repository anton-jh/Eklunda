using Eklunda.Web.Core.Intentions;
using Eklunda.Web.Core.Notifications;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Shared;

namespace Eklunda.Web.Modules.Heating;

public static class HeatingEndpoints
{
    public static IEndpointRouteBuilder MapHeatingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(HeatingNotifier.TakeNextUrl, async (
            HeatingService heatingService, IntentionNotifier intentionNotifier, UserService userService,
            ICurrentUser currentUser, IUnitOfWork unitOfWork, CancellationToken cancellationToken) =>
        {
            var (intention, created) = await heatingService.TakeNext(currentUser.Id, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            if (created)
                await intentionNotifier.NotifyCreated(intention, cancellationToken);

            if (intention.CreatedBy == currentUser.Id)
                return new PushActionResult("Du tar nästa ✓", "Uppvärmningen");

            var names = await userService.GetNames(cancellationToken);
            var name = names.NameOf(intention.CreatedBy);
            return new PushActionResult($"{name} hann före", "Uppvärmningen");
        });

        return app;
    }
}
