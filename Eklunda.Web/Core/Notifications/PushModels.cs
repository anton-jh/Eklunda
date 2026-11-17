using Eklunda.Web.Core.Users;

namespace Eklunda.Web.Core.Notifications;

public class DevicePushSubscription(UserId userId, string endpoint, string p256dh, string auth)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTimeOffset CreatedDate { get; private set; } = DateTimeOffset.UtcNow;
    public UserId UserId { get; set; } = userId;
    public string Endpoint { get; private set; } = endpoint;
    public string P256dh { get; set; } = p256dh;
    public string Auth { get; set; } = auth;
}

public class VapidKeys(string publicKey, string privateKey)
{
    public int Id { get; private set; } = 1;
    public string PublicKey { get; private set; } = publicKey;
    public string PrivateKey { get; private set; } = privateKey;
}

// Shown by wwwroot/sw.js. Notifications with the same tag replace each other.
// DismissUrl is POSTed when the notification is swiped away or tapped (not when a button is used).
public record PushNotification(
    string Title,
    string Body,
    string Tag,
    string Url,
    IReadOnlyList<PushAction>? Actions = null,
    string? DismissUrl = null);

// The service worker POSTs to PostUrl and shows the returned PushActionResult.
public record PushAction(string Action, string Title, string PostUrl);

public record PushActionResult(string Title, string Body);
