namespace Buggy;

using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

public static class WorkItemExtensions
{
    public static Model.WorkItem ToBuggyModel(this WorkItem w)
    {
        return new Model.WorkItem((long)w.Fields["System.Id"], (string)w.Fields["System.Title"], (string)w.Fields["System.WorkItemType"], (string)w.Fields["System.State"]);
    }
}
