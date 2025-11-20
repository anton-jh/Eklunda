namespace Eklunda.Web.Core.Events;

public interface IEventListener
{
    Task Process(IEvent e);
}
