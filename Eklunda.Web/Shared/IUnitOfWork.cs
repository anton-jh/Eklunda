namespace Eklunda.Web.Shared;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken);
}
