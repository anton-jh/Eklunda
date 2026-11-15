namespace Eklunda.Web.Core.Users;

public class User(string name)
{
    public UserId Id { get; private set; } = new(Guid.NewGuid());
    public DateTimeOffset CreatedDate { get; private set; } = DateTimeOffset.UtcNow;
    public string Name { get; set; } = name;
}

public record UserId(Guid Value);
