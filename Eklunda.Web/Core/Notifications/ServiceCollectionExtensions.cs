namespace Eklunda.Web.Core.Notifications;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPushNotifications(this IServiceCollection services)
    {
        services.AddHttpClient(nameof(PushNotifier));
        services.AddSingleton<VapidKeyProvider>();
        services.AddScoped<PushNotifier>();
        return services;
    }
}
