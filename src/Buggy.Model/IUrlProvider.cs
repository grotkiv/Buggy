namespace Buggy.Model;

public interface IUrlProvider
{
    string GetProjectOverviewUrl();

    string GetWorkItemUrl(WorkItem workItem);
}