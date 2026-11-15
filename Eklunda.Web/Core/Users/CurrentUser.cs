namespace Eklunda.Web.Core.Users;

public interface ICurrentUser
{
    UserId Id { get; }
}

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public UserId Id => UserAuthentication.GetUserId(httpContextAccessor.HttpContext?.User)
        ?? throw new InvalidOperationException("No user has been chosen for this request.");
}
