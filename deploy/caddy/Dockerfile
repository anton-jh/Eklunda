# Caddy with the Cloudflare DNS plugin, so it can get Let's Encrypt certificates for names that
# point at the Pi's local IP. Build from this folder:
#   docker buildx build --platform linux/arm64 -t caddy-cloudflare --load .
# The Go build runs natively and cross-compiles, so building for the Pi is fast on a PC.

FROM --platform=$BUILDPLATFORM caddy:2-builder AS build
ARG TARGETARCH
RUN CGO_ENABLED=0 GOOS=linux GOARCH=$TARGETARCH xcaddy build --with github.com/caddy-dns/cloudflare

FROM caddy:2
COPY --from=build /usr/bin/caddy /usr/bin/caddy
