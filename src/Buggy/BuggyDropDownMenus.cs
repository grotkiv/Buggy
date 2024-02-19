namespace Buggy;

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Buggy.Model;
using Microsoft.VisualStudio.Services.Common;

public interface IBuggyDropDownMenus
{
    ContextMenuStrip GetMenuStrip(string workItemType);
}

public class BuggyDropDownMenus : IBuggyDropDownMenus
{
    private readonly Dictionary<string, ContextMenuStrip> menuMap = new();

    public BuggyDropDownMenus(IBuggyActions buggyActions)
    {
        InitMenuMap(
            buggyActions,
            WorkItemType.UserStory,
            WorkItemType.Bug,
            WorkItemType.Issue,
            WorkItemType.Task,
            WorkItemType.TestCase
        );
    }

    public ContextMenuStrip GetMenuStrip(string workItemType)
    {
        if (menuMap.TryGetValue(Redirect(workItemType), out var result))
        {
            return result;
        }

        throw new InvalidOperationException($"There is no menu registered for work item type '{workItemType}'");
    }

    private void InitMenuMap(IBuggyActions buggyActions, params string[] workItemTypes)
    {
        foreach (var workItemType in workItemTypes)
        {
            var menu = new ContextMenuStrip();
            buggyActions.GetActionMap(workItemType).ForEach(action => menu.Items.Add(action.Key));
            menuMap[workItemType] = menu;
        }
    }

    /// <summary>
    /// Redirects Epic and Feature to User Story, because they share the same state values.
    /// </summary>
    /// <param name="workItemType">The type string to redirect.</param>
    /// <returns><see cref="WorkItemType.UserStory"> for Epic and Feature otherwise <paramref name="workItemType"/>.</returns>
    private string Redirect(string workItemType)
    {
        if (WorkItemType.Feature.Equals(workItemType) || WorkItemType.Epic.Equals(workItemType))
        {
            return WorkItemType.UserStory;
        }

        return workItemType;
    }
}
