namespace Buggy.Azure;

using Buggy.Model;
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var watch = new Stopwatch();

        logger.LogInformation("WIQL Query: {@WiqlQuery}", project.Query);
        logger.LogInformation("UpdatePeriod: {@UpdatePeriod}", project.UpdatePeriod);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                watch.Restart();
                var azureWorkItems = await RunWiqlQuery()
                    .ConfigureAwait(false);
                logger.LogDebug("WIQL Query duration: {@WiqlDuration}", watch.ElapsedMilliseconds);

                watch.Restart();
                JoinWorkItems(azureWorkItems);
                logger.LogDebug("Collection update duration: {@CollectionUpdateDuration}", watch.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Unexpected exception while collecting work items");
            }
            finally
            {
                await Delay(project.UpdatePeriod, stoppingToken)
                    .ConfigureAwait(false);
            }
        }
    }

    private async Task<IEnumerable<Model.WorkItem>> RunWiqlQuery()
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

    private void JoinWorkItems(IEnumerable<Model.WorkItem> azureWorkItems)
    {
        workItems.RemoveAll(item => !azureWorkItems.Any(bwi => bwi.Id == item.Id));

        azureWorkItems.ForEach(freshWorkItem =>
        {
            if (workItems.SingleOrDefault(current => current.Id == freshWorkItem.Id) is Model.WorkItem item)
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
    }

    private async Task Delay(TimeSpan delay, CancellationToken stoppingToken = default)
    {
        try
        {
            await Task.Delay(delay, stoppingToken)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogDebug(e, "Task.Delay failed or was canceled.");
        }
    }
}