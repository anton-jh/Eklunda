// Service worker and push subscription. Push needs https (or localhost).
(() => {
    const supported = "serviceWorker" in navigator && "PushManager" in window && "Notification" in window;
    const registration = "serviceWorker" in navigator
        ? navigator.serviceWorker.register("/sw.js")
        : Promise.reject(new Error("Service workers are not supported"));
    registration.catch(e => console.warn("Service worker:", e));

    const currentUser = document.querySelector("meta[name=eklunda-user]")?.content ?? "";

    function base64UrlToBytes(value) {
        const base64 = (value + "=".repeat((4 - value.length % 4) % 4)).replace(/-/g, "+").replace(/_/g, "/");
        return Uint8Array.from(atob(base64), c => c.charCodeAt(0));
    }

    async function post(url, body) {
        const response = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });
        if (!response.ok) throw new Error(`${url}: ${response.status}`);
    }

    async function save(subscription) {
        await post("/api/push/subscriptions", subscription.toJSON());
        try { localStorage.setItem("eklunda-push-synced", `${currentUser}|${subscription.endpoint}`); } catch { }
    }

    async function getSubscription() {
        const reg = await registration;
        return reg.pushManager.getSubscription();
    }

    window.eklundaPush = {
        // "unsupported" | "denied" | "off" | "on"
        async state() {
            if (!supported) return "unsupported";
            if (Notification.permission === "denied") return "denied";
            if (Notification.permission !== "granted") return "off";
            return (await getSubscription()) ? "on" : "off";
        },

        async enable(publicKey) {
            if (await Notification.requestPermission() !== "granted") return;
            const reg = await registration;
            const subscription = await reg.pushManager.getSubscription()
                ?? await reg.pushManager.subscribe({
                    userVisibleOnly: true,
                    applicationServerKey: base64UrlToBytes(publicKey)
                });
            await save(subscription);
        },

        async disable() {
            const subscription = await getSubscription();
            if (!subscription) return;
            await post("/api/push/unsubscribe", { endpoint: subscription.endpoint });
            await subscription.unsubscribe();
        }
    };

    // Re-save when the device switched user or the browser replaced the subscription.
    if (supported && currentUser && Notification.permission === "granted") {
        getSubscription().then(subscription => {
            if (!subscription) return;
            let synced = "";
            try { synced = localStorage.getItem("eklunda-push-synced") ?? ""; } catch { }
            if (synced !== `${currentUser}|${subscription.endpoint}`) return save(subscription);
        }).catch(e => console.warn("Push sync:", e));
    }
})();
