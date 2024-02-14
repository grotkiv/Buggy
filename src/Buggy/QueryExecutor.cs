namespace Buggy;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;

public class QueryExecutor : BackgroundService
{
    private readonly AzureProject project;
    private readonly Uri uri;
    private readonly ObservableCollection<Model.WorkItem> workItems;
    private readonly ILogger<QueryExecutor> logger;

    public QueryExecutor(ObservableCollection<Model.WorkItem> workItems, IOptions<AzureProject> options, ILogger<QueryExecutor> logger)
    {
        project = options.Value;
        this.uri = new Uri(string.Join('/', project.Url, project.Organization));
        this.workItems = workItems;
        this.logger = logger;
    }

    /// <summary>
    ///     Execute a WIQL (Work Item Query Language) query to return a list of open bugs.
    /// </summary>
    /// <param name="project">The name of your project within your organization.</param>
    /// <returns>A list of <see cref="WorkItem"/> objects representing all the open bugs.</returns>
    public async Task<IEnumerable<WorkItem>> QueryOpenBugs()
    {
        var credentials = new VssBasicCredential(string.Empty, this.project.Pat);

        // create a wiql object and build our query
        var wiql = new Wiql()
        {
            // NOTE: Even if other columns are specified, only the ID & URL are available in the WorkItemReference
            Query = "Select [Id] " +
                    "From WorkItems " +
                    "Where [System.TeamProject] = '" + project.Project + "' " +
                    "And [System.State] <> 'Closed' " +
                    "And [System.AssignedTo] = @me " +
                    "Order By [State] Asc, [Changed Date] Desc",
        };

        // create instance of work item tracking http client
        using (var httpClient = new WorkItemTrackingHttpClient(this.uri, credentials))
        {
            // execute the query to get the list of work items in the results
            var result = await httpClient.QueryByWiqlAsync(wiql).ConfigureAwait(false);
            var ids = result.WorkItems.Select(item => item.Id).ToArray();

            // some error handling
            if (ids.Length == 0)
            {
                return Array.Empty<WorkItem>();
            }

            // build a list of the fields we want to see
            var fields = new[] { "System.Id", "System.Title", "System.State", "System.WorkItemType" };

            // get work items for the ids found in query
            return await httpClient.GetWorkItemsAsync(ids, fields, result.AsOf).ConfigureAwait(false);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var watch = new Stopwatch();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                watch.Restart();
                var azureWorkItems = await QueryOpenBugs().ConfigureAwait(false);
                logger.LogDebug("WIQL Query duration: {@WiqlDuration}", watch.ElapsedMilliseconds);

                watch.Restart();
                var freshWorkItems = azureWorkItems.Select(w => w.ToBuggyModel());

                workItems.RemoveAll(item => !freshWorkItems.Any(bwi => bwi.Id == item.Id));

                freshWorkItems.ForEach(freshWorkItem =>
                {
                    if(workItems.SingleOrDefault(current => current.Id == freshWorkItem.Id) is Model.WorkItem item)
                    {
                        item.State = freshWorkItem.State = item.State;
                        item.Title = freshWorkItem.Title = item.Title;
                        item.Type = freshWorkItem.Type = item.Type;
                    }
                    else
                    {
                        workItems.Add(freshWorkItem);
                    }
                });

                logger.LogDebug("Collection update duration: {@CollectionUpdateDuration}", watch.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Unexpected exception while collecting work items");
            }
            finally
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (Exception e)
                {
                    logger.LogDebug(e, "Task.Delay failed or was canceled.");
                }
            }
        }
    }
}