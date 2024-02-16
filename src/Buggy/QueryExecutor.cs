namespace Buggy;

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
using Microsoft.VisualStudio.Services.Common;

public class QueryExecutor : BackgroundService
{
    private readonly Settings settings;
    private readonly IWorkItemQuery query;
    private readonly ObservableCollection<WorkItem> workItems;
    private readonly ILogger<QueryExecutor> logger;

    public QueryExecutor(IWorkItemQuery query, IOptions<Settings> options, ObservableCollection<WorkItem> workItems, ILogger<QueryExecutor> logger)
    {
        settings = options.Value;
        this.query = query;
        this.workItems = workItems;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var watch = new Stopwatch();

        logger.LogInformation("UpdatePeriod: {@UpdatePeriod}", settings.UpdatePeriod);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                watch.Restart();
                var workItems = await query.RunQuery()
                    .ConfigureAwait(false);
                logger.LogDebug("WIQL Query duration: {@WiqlDuration}", watch.ElapsedMilliseconds);

                watch.Restart();
                JoinWorkItems(workItems);
                logger.LogDebug("Collection update duration: {@CollectionUpdateDuration}", watch.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Unexpected exception while collecting work items");
            }
            finally
            {
                await Delay(settings.UpdatePeriod, stoppingToken)
                    .ConfigureAwait(false);
            }
        }
    }

    private void JoinWorkItems(IEnumerable<WorkItem> azureWorkItems)
    {
        workItems.RemoveAll(item => !azureWorkItems.Any(bwi => bwi.Id == item.Id));

        azureWorkItems.ForEach(freshWorkItem =>
        {
            if (workItems.SingleOrDefault(current => current.Id == freshWorkItem.Id) is WorkItem item)
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