namespace Eklunda.Web.Core.Events;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEvents(this IServiceCollection services)
    {
        services.AddScoped<EventService>();

        var listenerTypes = typeof(ServiceCollectionExtensions).Assembly.GetTypes()
            .Where(x => x.IsAssignableTo(typeof(IEventListener)))
            .Except([typeof(IEventListener)]);

        foreach (var listenerType in listenerTypes)
        {
            services.AddTransient(typeof(IEventListener), listenerType);
        }

        return services;
    }
}
