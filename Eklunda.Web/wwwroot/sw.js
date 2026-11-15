// Payload: PushNotification in Core/Notifications/PushModels.cs.

const icon = "/icons/icon-192.png";
const badge = "/icons/badge-96.png";

self.addEventListener("install", () => self.skipWaiting());
self.addEventListener("activate", event => event.waitUntil(self.clients.claim()));

self.addEventListener("push", event => {
    const data = event.data?.json() ?? {};
    const actions = data.actions ?? [];
    event.waitUntil(self.registration.showNotification(data.title ?? "Eklunda", {
        body: data.body ?? "",
        tag: data.tag,
        renotify: Boolean(data.tag),
        icon,
        badge,
        actions: actions.map(a => ({ action: a.action, title: a.title })),
        data: { url: data.url ?? "/", actions }
    }));
});

self.addEventListener("notificationclick", event => {
    const notification = event.notification;
    const { url, actions = [] } = notification.data ?? {};
    notification.close();

    const action = actions.find(a => a.action === event.action);
    event.waitUntil(action ? runAction(action, notification.tag, url) : openPage(url ?? "/"));
});

async function runAction(action, tag, url) {
    let result = { title: "Det gick inte", body: "Försök igen i appen." };
    try {
        const response = await fetch(action.postUrl, { method: "POST", credentials: "same-origin" });
        if (response.ok) result = await response.json();
    } catch { }

    await self.registration.showNotification(result.title, {
        body: result.body ?? "",
        tag,
        icon,
        badge,
        data: { url, actions: [] }
    });
}

async function openPage(url) {
    const [client] = await self.clients.matchAll({ type: "window" });
    if (!client) return self.clients.openWindow(url);
    await client.focus();
    return client.navigate(url);
}
