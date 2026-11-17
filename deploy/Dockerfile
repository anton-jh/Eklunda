# Build from the repository root:
#   docker buildx build --platform linux/arm64 -f deploy/Dockerfile -t eklunda .
# The SDK stage runs natively and cross-compiles, so building for the Pi is fast on a PC.

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY Eklunda.Web/Eklunda.Web.csproj Eklunda.Web/
RUN dotnet restore Eklunda.Web -a $TARGETARCH
COPY Eklunda.Web/ Eklunda.Web/
RUN dotnet publish Eklunda.Web -c Release -a $TARGETARCH --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# Mount point for the Data Protection keys volume, owned by the app user.
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys && chown -R $APP_UID /home/app/.aspnet
USER $APP_UID
ENTRYPOINT ["./Eklunda.Web"]
