namespace Buggy;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Buggy.Model;

public interface IBuggyActions
{
    ImmutableDictionary<string, Func<WorkItem, Task>> GetActionMap(string workItemType);
}

public class BuggyActions : IBuggyActions
{
    private const string Activate = "Activate";
    private const string Close = "Close";
    private const string Resolve = "Resolve";
    private const string Remove = "Remove";
    private const string New = "New";
    private const string Design = "Design";
    private const string Ready = "Ready";

    private readonly ImmutableDictionary<string, Func<WorkItem, Task>> storyActionMap;
    private readonly ImmutableDictionary<string, Func<WorkItem, Task>> taskActionMap;
    private readonly ImmutableDictionary<string, Func<WorkItem, Task>> bugActionMap;
    private readonly ImmutableDictionary<string, Func<WorkItem, Task>> issueActionMap;
    private readonly ImmutableDictionary<string, Func<WorkItem, Task>> testCaseActionMap;

    public BuggyActions(IWorkItemActions workItemActions)
    {
        // Epic, Feature, User Story
        storyActionMap = new Dictionary<string, Func<WorkItem, Task>>()
        {
            { Activate, workItemActions.ActivateAsync },
            { Close, workItemActions.CloseAsync },
            { Resolve, workItemActions.ResolveAsync },
            { Remove, workItemActions.RemoveAsync },
            { New, workItemActions.NewAsync },
        }.ToImmutableDictionary();

        // Task
        taskActionMap = new Dictionary<string, Func<WorkItem, Task>>()
        {
            { Activate, workItemActions.ActivateAsync },
            { Close, workItemActions.CloseAsync },
            { Remove, workItemActions.RemoveAsync },
            { New, workItemActions.NewAsync },
        }.ToImmutableDictionary();

        // Bug
        bugActionMap = new Dictionary<string, Func<WorkItem, Task>>()
        {
            { Activate, workItemActions.ActivateAsync },
            { Close, workItemActions.CloseAsync },
            { Resolve, workItemActions.ResolveAsync },
            { New, workItemActions.NewAsync },
        }.ToImmutableDictionary();

        // Issue
        issueActionMap = new Dictionary<string, Func<WorkItem, Task>>()
        {
            { Activate, workItemActions.ActivateAsync },
            { Close, workItemActions.CloseAsync },
        }.ToImmutableDictionary();

        // Test Case
        testCaseActionMap = new Dictionary<string, Func<WorkItem, Task>>()
        {
            { Design, workItemActions.DesignAsync },
            { Ready, workItemActions.ReadyAsync },
            { Close, workItemActions.CloseAsync },
        }.ToImmutableDictionary();
    }

    public ImmutableDictionary<string, Func<WorkItem, Task>> GetActionMap(string workItemType)
    {
        switch (workItemType)
        {
            case WorkItemType.Epic:
            case WorkItemType.Feature:
            case WorkItemType.UserStory:
                return storyActionMap;
            case WorkItemType.Bug:
                return bugActionMap;
            case WorkItemType.Issue:
                return issueActionMap;
            case WorkItemType.Task:
                return taskActionMap;
            case WorkItemType.TestCase:
                return testCaseActionMap;
            default:
                throw new InvalidOperationException($"No actions registered for work item type '{workItemType}'");
        }
    }
}
