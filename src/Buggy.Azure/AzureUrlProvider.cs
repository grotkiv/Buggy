namespace Buggy.Azure;

using Buggy.Model;
using Microsoft.Extensions.Options;

public class AzureUrlProvider : IUrlProvider
{
    private readonly AzureProject project;

    public AzureUrlProvider(IOptions<AzureProject> options)
    {
        project = options.Value;
    }

    public string GetProjectOverviewUrl()
    {
        return string.Join('/', project.Url, project.Organization, project.Project);
    }

    public string GetWorkItemUrl(WorkItem workItem)
    {
        return string.Join('/', GetProjectOverviewUrl(), "_workitems", "edit", workItem.Id);
    }
}