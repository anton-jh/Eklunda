namespace Eklunda.Web.Core.Users;

public class User
{
    public UserId Id { get; private set; } = new(Guid.NewGuid());
}

public record UserId(Guid Value)
{
    public static UserId Example1 { get; } = new(Guid.Parse("aa2e9828-6e05-4ab8-948d-3689007a90eb"));
}
