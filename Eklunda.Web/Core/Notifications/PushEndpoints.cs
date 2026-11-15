using Eklunda.Web.Core.Users;
using Eklunda.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Eklunda.Web.Core.Notifications;

public static class PushEndpoints
{
    public record SubscriptionKeys(string P256dh, string Auth);
    public record SubscriptionRequest(string Endpoint, SubscriptionKeys Keys);
    public record UnsubscribeRequest(string Endpoint);

    public static IEndpointRouteBuilder MapPushEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/push");

        // Upsert: the device may have switched user since it subscribed.
        group.MapPost("/subscriptions", async (
            SubscriptionRequest request, ApplicationDbContext dbContext, ICurrentUser currentUser,
            CancellationToken cancellationToken) =>
        {
            if (!Uri.TryCreate(request.Endpoint, UriKind.Absolute, out var endpoint) || endpoint.Scheme != Uri.UriSchemeHttps)
                return Results.BadRequest();

            var subscription = await dbContext.PushSubscriptions
                .SingleOrDefaultAsync(x => x.Endpoint == request.Endpoint, cancellationToken);

            if (subscription is null)
            {
                dbContext.PushSubscriptions.Add(new DevicePushSubscription(
                    currentUser.Id, request.Endpoint, request.Keys.P256dh, request.Keys.Auth));
            }
            else
            {
                subscription.UserId = currentUser.Id;
                subscription.P256dh = request.Keys.P256dh;
                subscription.Auth = request.Keys.Auth;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        });

        group.MapPost("/unsubscribe", async (
            UnsubscribeRequest request, ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
        {
            await dbContext.PushSubscriptions
                .Where(x => x.Endpoint == request.Endpoint)
                .ExecuteDeleteAsync(cancellationToken);
            return Results.NoContent();
        });

        return app;
    }
}
