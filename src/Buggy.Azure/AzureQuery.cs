namespace Buggy.Azure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buggy.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;

public class AzureQuery : IWorkItemQuery
{
    private readonly AzureProject project;
    private readonly Uri uri;
    private readonly ILogger<AzureQuery> logger;

    public AzureQuery(IOptions<AzureProject> options, ILogger<AzureQuery> logger)
    {
        project = options.Value;
        this.uri = new Uri(string.Join('/', project.Url, project.Organization));
        this.logger = logger;

        logger.LogInformation("Azure URI: {@AzureUri}", uri);
        logger.LogInformation("Azure WIQL Query: {@WiqlQuery}", project.Query);
    }

    public async Task<IEnumerable<Model.WorkItem>> RunQuery()
    {
        var credentials = new VssBasicCredential(string.Empty, project.Pat);
        using var httpClient = new WorkItemTrackingHttpClient(this.uri, credentials);

        var wiql = new Wiql() { Query = project.Query };
        var result = await httpClient.QueryByWiqlAsync(wiql)
            .ConfigureAwait(false);
        if (!result.WorkItems.Any())
        {
            return Array.Empty<Model.WorkItem>();
        }

        var ids = result.WorkItems.Select(item => item.Id);
        var fields = new[] { "System.Id", "System.Title", "System.State", "System.WorkItemType" };
        var azureWorkItems = await httpClient.GetWorkItemsAsync(ids, fields, result.AsOf)
            .ConfigureAwait(false);

        return azureWorkItems.Select(w => w.ToBuggyModel());
    }
}
