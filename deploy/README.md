# Running Eklunda on the Raspberry Pi

Eklunda runs on the local network behind [Caddy](https://caddyserver.com), which serves it over
HTTPS using its own local certificate authority. HTTPS is needed for push notifications and for
installing the app on the phone's home screen. Browsers only allow those on secure origins.

```
phone ──https──▶ Caddy (:443, tls internal) ──http──▶ Eklunda (localhost:5000) ──▶ PostgreSQL
```

## 1. Give the Pi a fixed address

Reserve an IP for the Pi in the router (e.g. `192.168.1.50`). If the router can resolve names
(e.g. `eklunda.home`), set that up too. Use the same address(es) in the `Caddyfile`.

## 2. Run the app

Publish for the Pi and copy it over:

```bash
dotnet publish Eklunda.Web -c Release -r linux-arm64 --self-contained -o publish
rsync -a publish/ pi@192.168.1.50:/opt/eklunda/
```

Apply database migrations (from the dev machine, pointing at the Pi's database, or using a
migration bundle: `dotnet ef migrations bundle`). Then install and start the service:
see `eklunda.service`.

## 3. Caddy

```bash
sudo apt install caddy
sudo cp deploy/Caddyfile /etc/caddy/Caddyfile   # edit the addresses first
sudo systemctl reload caddy
```

## 4. Trust Caddy's certificate on each phone (once per phone)

Caddy's root certificate is valid for about 10 years. The certificates it signs renew automatically.

1. Copy the root certificate somewhere the phone can download it, e.g.:
   ```bash
   sudo cp /var/lib/caddy/.local/share/caddy/pki/authorities/local/root.crt ~/eklunda-root.crt
   ```
   (Send it to the phone any way you like. It's a public certificate, not a secret.)
2. On Android: **Settings → Security & privacy → More security settings → Encryption &
   credentials → Install a certificate → CA certificate**, then pick the file. Menu names differ
   a bit between phone makers. Search settings for "CA certificate".
3. Open `https://192.168.1.50` in Chrome. There should be no warning.
4. Chrome menu → **Add to home screen / Install app**.
5. In Eklunda: pick your name, then **Inställningar → Slå på notiser**.

## Notes

- Push notifications go through Google's push service, so the Pi needs outbound internet for
  them. The app itself works on the local network without internet.
- The VAPID keys for push are generated on first use and stored in the database (`VapidKeys`).
  If that row is lost, everyone has to turn notifications on again.
- Login cookies are protected with ASP.NET Data Protection keys stored in the service user's
  home directory (`~/.aspnet/DataProtection-Keys`). If they are lost, everyone just picks their
  name again.
