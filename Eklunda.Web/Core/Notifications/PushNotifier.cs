using System.Net;
using System.Text.Json;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Persistence;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Eklunda.Web.Core.Notifications;

public class PushNotifier(
    ApplicationDbContext dbContext,
    VapidKeyProvider vapidKeyProvider,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<PushNotifier> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task SendTo(UserId user, PushNotification notification, CancellationToken cancellationToken)
        => Send(dbContext.PushSubscriptions.Where(x => x.UserId == user), notification, cancellationToken);

    public Task SendToAllExcept(UserId? except, PushNotification notification, CancellationToken cancellationToken)
        => Send(dbContext.PushSubscriptions.Where(x => x.UserId != except), notification, cancellationToken);

    private async Task Send(
        IQueryable<DevicePushSubscription> query, PushNotification notification, CancellationToken cancellationToken)
    {
        var subscriptions = await query.ToListAsync(cancellationToken);
        if (subscriptions.Count == 0)
            return;

        var keys = await vapidKeyProvider.Get(cancellationToken);
        using var vapid = new VapidAuthentication(keys.PublicKey, keys.PrivateKey)
        {
            Subject = configuration["Push:Subject"] ?? "mailto:admin@eklunda.local",
        };
        var client = new PushServiceClient(httpClientFactory.CreateClient(nameof(PushNotifier)))
        {
            DefaultAuthentication = vapid,
        };

        var content = JsonSerializer.Serialize(notification, JsonOptions);
        var gone = new List<DevicePushSubscription>();

        foreach (var subscription in subscriptions)
        {
            var target = new PushSubscription { Endpoint = subscription.Endpoint };
            target.SetKey(PushEncryptionKeyName.P256DH, subscription.P256dh);
            target.SetKey(PushEncryptionKeyName.Auth, subscription.Auth);

            try
            {
                await client.RequestPushMessageDeliveryAsync(target, new PushMessage(content)
                {
                    Topic = notification.Tag,
                    TimeToLive = (int)TimeSpan.FromHours(12).TotalSeconds,
                    Urgency = PushMessageUrgency.High,
                }, cancellationToken);
            }
            catch (PushServiceClientException ex) when (ex.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound)
            {
                gone.Add(subscription);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Push to subscription {SubscriptionId} failed", subscription.Id);
            }
        }

        if (gone.Count > 0)
        {
            dbContext.PushSubscriptions.RemoveRange(gone);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
