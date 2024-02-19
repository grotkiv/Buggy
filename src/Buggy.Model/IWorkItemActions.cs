namespace Buggy.Model;

using System.Threading.Tasks;

public interface IWorkItemActions
{
    Task NewAsync(WorkItem workItem);

    Task ActivateAsync(WorkItem workItem);

    Task ResolveAsync(WorkItem workItem);

    Task CloseAsync(WorkItem workItem);

    Task RemoveAsync(WorkItem workItem);

    Task DesignAsync(WorkItem workItem);

    Task ReadyAsync(WorkItem workItem);
}