using Eklunda.Web.Persistence;
using Eklunda.Web.Shared;
using Microsoft.EntityFrameworkCore;

namespace Eklunda.Web.Core.Users;

public class UserService(ApplicationDbContext dbContext)
{
    public const int MaxNameLength = 40;

    public Task<List<User>> GetAll(CancellationToken cancellationToken)
        => dbContext.Users.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<User?> Get(UserId id, CancellationToken cancellationToken)
        => dbContext.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyDictionary<UserId, string>> GetNames(CancellationToken cancellationToken)
        => await dbContext.Users.ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

    public async Task<User> GetOrCreate(string name, CancellationToken cancellationToken)
    {
        name = Clean(name);
        var users = await dbContext.Users.ToListAsync(cancellationToken);
        var existing = users.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        if (existing is not null)
            return existing;

        var user = new User(name);
        dbContext.Users.Add(user);
        return user;
    }

    public static string Clean(string? name) => Strings.Clean(name, MaxNameLength);
}

public static class UserNameExtensions
{
    public static string NameOf(this IReadOnlyDictionary<UserId, string> names, UserId? id)
        => id is not null && names.TryGetValue(id, out var name) ? name : "Någon";

    public static string NameOf(this IReadOnlyDictionary<UserId, string> names, UserId? id, UserId me)
        => id == me ? "Du" : names.NameOf(id);
}
