namespace Buggy.Azure;

using System.Threading.Tasks;
using Buggy.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

public class AzureActions : IWorkItemActions
{
    private readonly AzureProject project;
    private readonly ILogger<AzureActions> logger;

    public AzureActions(IOptions<AzureProject> options, ILogger<AzureActions> logger)
    {
        project = options.Value;
        this.logger = logger;
    }

    public Task ActivateAsync(WorkItem workItem) => UpdateStateAsync(workItem, "Active");

    public Task CloseAsync(WorkItem workItem) => UpdateStateAsync(workItem, "Closed");

    public Task DesignAsync(WorkItem workItem) => UpdateStateAsync(workItem, "Design");

    public Task NewAsync(WorkItem workItem) => UpdateStateAsync(workItem, "New");

    public Task ReadyAsync(WorkItem workItem) => UpdateStateAsync(workItem, "Ready");

    public Task RemoveAsync(WorkItem workItem) => UpdateStateAsync(workItem, "Removed");

    public Task ResolveAsync(WorkItem workItem) => UpdateStateAsync(workItem, "Resolved");

    private async Task UpdateStateAsync(WorkItem workItem, string value)
    {
        var credentials = new VssBasicCredential(string.Empty, project.Pat);
        using var httpClient = new WorkItemTrackingHttpClient(project.OrganizationUrl, credentials);

        var document = new JsonPatchDocument
        {
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.State",
                Value = value,
            }
        };

        await httpClient.UpdateWorkItemAsync(document, (int)workItem.Id)
            .ConfigureAwait(false);
    }
}