using Eklunda.Web.Core.Intentions;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Heating.Models;
using Eklunda.Web.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Eklunda.Web.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .WrappedGuidConversion<IntentionId>()
            .WrappedGuidConversion<UserId>();
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return SaveChangesAsync(cancellationToken);
    }

    // Core
    public required DbSet<Intention> Intentions { get; init; }

    // Heating
    public required DbSet<HeatingLogEntry> HeatingLogEntries { get; init; }
}


// not sure if this works
public class SimpleValueTypeConverter<TModel, TProvider> : ValueConverter<TModel?, TProvider?>
    where TModel : class
{
    public SimpleValueTypeConverter() : base(
        x => (TProvider?)(x == null ? null : x.GetType().GetProperty("Value")!.GetValue(x)),
        x => x == null ? null : (TModel)typeof(TModel).GetConstructor(new Type[] { typeof(TProvider) })!.Invoke(new object[] { x }))
    {
    }
}

public static class DbContextExtensions
{
    public static ModelConfigurationBuilder ValueTypeConversion<TModel, TProvider>(this ModelConfigurationBuilder configurationBuilder)
        where TModel : class
    {
        configurationBuilder.Properties<TModel>()
            .HaveConversion<SimpleValueTypeConverter<TModel, TProvider>>();

        return configurationBuilder;
    }

    public static ModelConfigurationBuilder WrappedGuidConversion<TModel>(this ModelConfigurationBuilder configurationBuilder)
        where TModel : class
    {
        configurationBuilder.Properties<TModel>()
            .HaveConversion<SimpleValueTypeConverter<TModel, Guid>>();

        return configurationBuilder;
    }
}
