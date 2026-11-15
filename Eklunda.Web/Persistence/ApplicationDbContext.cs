using Eklunda.Web.Core.Intentions;
using Eklunda.Web.Core.Notifications;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(UserService.MaxNameLength);
        });

        modelBuilder.Entity<Intention>(b =>
        {
            b.Property(x => x.Description).HasMaxLength(IntentionService.MaxDescriptionLength);
            b.HasIndex(x => new { x.DoneWhenEvent, x.DoneAt });
        });

        modelBuilder.Entity<DevicePushSubscription>(b =>
        {
            b.HasIndex(x => x.Endpoint).IsUnique();
        });

        modelBuilder.Entity<VapidKeys>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();
        });
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return SaveChangesAsync(cancellationToken);
    }

    // Core
    public required DbSet<User> Users { get; init; }
    public required DbSet<Intention> Intentions { get; init; }
    public required DbSet<DevicePushSubscription> PushSubscriptions { get; init; }
    public required DbSet<VapidKeys> VapidKeys { get; init; }

    // Heating
    public required DbSet<HeatingLogEntry> HeatingLogEntries { get; init; }
}


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
