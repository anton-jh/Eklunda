# Running Eklunda on the Raspberry Pi

Eklunda runs in Docker on the Pi, behind a shared [Caddy](https://caddyserver.com) container that
serves every service on the Pi over HTTPS. HTTPS is needed for push notifications and for
installing the app on the phone's home screen.

```
phone ──https──▶ Caddy (:443) ──web network──▶ eklunda (:8080) ──▶ db (PostgreSQL)
```

`skogeklunda.se` is a public DNS record (at Cloudflare) that points at the Pi's local IP.
Caddy gets a Let's Encrypt certificate for it through a DNS challenge, so the Pi is never
reachable from the internet and the phones need no extra setup. The app only works on the home
network. Push notifications arrive anywhere.

Images are built on the PC and copied to the Pi. Building on the Pi itself is slow.

## Once: Cloudflare

1. Add `skogeklunda.se` to Cloudflare (Free plan). At the registrar, turn off DNSSEC, then
   switch the domain's nameservers to Cloudflare's.
2. DNS record: type `A`, name `@`, the Pi's fixed IP, proxy status **DNS only** (grey cloud).
3. **My Profile → API Tokens → Create Token → Edit zone DNS**. Under **Zone Resources**, pick
   **Include → Specific zone → skogeklunda.se**.

## Once: Caddy

On the PC, from `deploy/caddy`:

```bash
docker buildx build --platform linux/arm64 -t caddy-cloudflare --load .
docker save -o caddy-cloudflare.tar caddy-cloudflare
ssh <user>@eklunda.local mkdir -p services/caddy
scp caddy-cloudflare.tar compose.yaml Caddyfile <user>@eklunda.local:services/caddy/
```

On the Pi:

```bash
cd ~/services/caddy
echo "CLOUDFLARE_API_TOKEN=<token>" > .env && chmod 600 .env
docker network create web
docker load -i caddy-cloudflare.tar
docker compose up -d
```

## Deploy Eklunda

On the PC, from the repository root:

```bash
docker buildx build --platform linux/arm64 -f deploy/Dockerfile -t eklunda --load .
docker save -o eklunda.tar eklunda
ssh <user>@eklunda.local mkdir -p services/eklunda
scp eklunda.tar deploy/compose.yaml <user>@eklunda.local:services/eklunda/
```

On the Pi, the first time:

```bash
echo "POSTGRES_PASSWORD=$(openssl rand -hex 16)" > ~/services/eklunda/.env
```

Then, on every deploy:

```bash
cd ~/services/eklunda
docker load -i eklunda.tar
docker compose up -d
```

Database migrations run when the app starts.

## On each phone

1. Open `https://skogeklunda.se` in Chrome.
2. Chrome menu → **Add to home screen / Install app**.
3. In Eklunda: pick your name, then **Inställningar → Slå på notiser**.

## Backup

```bash
docker compose exec db pg_dump -U eklunda eklunda > eklunda-$(date +%F).sql
```

## Notes

- Push notifications go through Google's push service, so the Pi needs outbound internet for
  them. Certificate renewal also needs it.
- The VAPID keys for push are generated on first use and stored in the database (`VapidKeys`).
  If that row is lost, everyone has to turn notifications on again.
- Login cookies are protected with ASP.NET Data Protection keys stored in the `keys` volume.
  If it is lost, everyone just picks their name again.
- The app trusts `X-Forwarded-*` headers from any address. That's fine as long as it is only
  reachable through Caddy, so don't publish its port.
