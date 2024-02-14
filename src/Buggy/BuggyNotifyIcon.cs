namespace Buggy;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Buggy.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;

public class BuggyNotifyIcon
{
    /// <summary>
    /// The minimum time period typically enforced by the operating system is 10s.
    /// </summary>
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.notifyicon.showballoontip?view=windowsdesktop-8.0" />
    /// <see cref="NotifyIcon.ShowBalloonTip(int, string, string, ToolTipIcon)"/>
    private const int minBalloonTipTimePeriodInSeconds = 10;

    private readonly NotifyIcon notifyIcon = new();
    private readonly ContextMenuStrip contextMenu = new();
    private readonly ThrottleFilter throttleFilter = new(TimeSpan.FromSeconds(minBalloonTipTimePeriodInSeconds));
    private readonly ObservableCollection<WorkItem> workItems;
    private readonly AzureProject azureProject;
    private readonly IHostApplicationLifetime lifetime;
    private readonly ILogger<BuggyNotifyIcon> logger;

    private static readonly Bitmap ImgEpic = Svg.Load("images/diamond-outline.svg", Color.DarkOrange);
    private static readonly Bitmap ImgFeature = Svg.Load("images/trophy-outline.svg", Color.DarkMagenta);
    private static readonly Bitmap ImgUserStory = Svg.Load("images/chatbox-outline.svg", Color.DeepSkyBlue);
    private static readonly Bitmap ImgBug = Svg.Load("images/bug-outline.svg", Color.IndianRed);
    private static readonly Bitmap ImgTask = Svg.Load("images/file-tray-full-outline.svg", Color.Goldenrod);
    private static readonly Bitmap ImgIssue = Svg.Load("images/bandage-outline.svg", Color.Magenta);
    private static readonly Bitmap ImgTestCase = Svg.Load("images/flask-outline.svg", Color.LightGreen);
    private static readonly Bitmap ImgSkull = Svg.Load("images/skull-outline.svg");

    public BuggyNotifyIcon(ObservableCollection<WorkItem> workItems, IOptions<AzureProject> azureOptions, IHostApplicationLifetime lifetime, ILogger<BuggyNotifyIcon> logger)
    {
        this.workItems = workItems;
        this.azureProject = azureOptions.Value;
        this.lifetime = lifetime;
        this.logger = logger;
        lifetime.ApplicationStopping.Register(() => Application.Exit());
    }

    public void Show()
    {
        contextMenu.Items.Add("&Exit", "images/exit-outline.svg", OnClickExit);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add($"Dashboard ({azureProject})", "images/clipboard-outline.svg", OnClickDashboard);
        contextMenu.Items.Add(new ToolStripSeparator());
        AddWorkItems(workItems);
        workItems.CollectionChanged += OnWorkItemCollectionChanged;

        notifyIcon.ContextMenuStrip = contextMenu;
        notifyIcon.Icon = Svg.LoadIcon("images/cart-outline.svg");
        notifyIcon.Visible = true;
    }

    private void OnWorkItemCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null && e.Action == NotifyCollectionChangedAction.Add)
        {
            AddWorkItems(e.NewItems.OfType<WorkItem>());
        }
        else if (e.OldItems != null && e.Action == NotifyCollectionChangedAction.Remove)
        {
            RemoveWorkItems(e.OldItems.OfType<WorkItem>());
        }
    }

    private void AddWorkItems(IEnumerable<WorkItem> workItems)
    {
        if (contextMenu.InvokeRequired)
        {
            contextMenu.Invoke(() => AddWorkItems(workItems));
        }
        else
        {
            var list = workItems.Select(workItem =>
            {
                workItem.PropertyChanged += OnWorkItemPropertyChanged;
                var menuItem = new ToolStripMenuItem(ToMenuItemText(workItem), GetWorkItemImage(workItem.Type), OnClickWorkItem, workItem.Id.ToString());
                menuItem.Tag = workItem;
                return menuItem;
            }).ToArray();

            contextMenu.Items.AddRange(list);
            ShowBalloonTipThrottled("New Work Items", "New work items match your query.");
        }
    }

    private void RemoveWorkItems(IEnumerable<WorkItem> workItems)
    {
        foreach (var workItem in workItems)
        {
            contextMenu.Items.RemoveByKey(workItem.Id.ToString());
        }
    }

    private void OnWorkItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is WorkItem workItem)
        {
            UpdateMenuItem(workItem);
        }
        else
        {
            logger.LogError("Not a WorkItem.");
        }
    }

    private void UpdateMenuItem(WorkItem workItem)
    {
        if (contextMenu.InvokeRequired)
        {
            contextMenu.Invoke(() => UpdateMenuItem(workItem));
        }
        else
        {
            var menuItem = contextMenu.Items[workItem.Id.ToString()] ?? throw new InvalidOperationException($"No menu item matches work item {workItem.Id}.");
            menuItem.Text = ToMenuItemText(workItem);
            menuItem.Tag = workItem;
            ShowBalloonTipThrottled($"{workItem.Type} {workItem.Id} changed", ToMenuItemText(workItem));
        }
    }

    private static string ToMenuItemText(WorkItem workItem)
    {
        return $"{workItem.Id} | {workItem.State} | {workItem.Title}";
    }

    /// <summary>
    /// Displays a balloon tip with the specified title, text, and icon in the taskbar for <see cref="minBalloonTipTimePeriodInSeconds"/>.
    /// But it calls <see cref="NotifyIcon.ShowBalloonTip(int, string, string, ToolTipIcon)"> every <see cref="minBalloonTipTimePeriodInSeconds"/> only.
    /// All other notification tries are ignored.
    /// </summary>
    /// <param name="title">The title to display on the balloon tip.</param>
    /// <param name="text">The text to display on the balloon tip.</param>
    /// <param name="icon">One of the ToolTipIcon values.</param>
    private void ShowBalloonTipThrottled(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
    {
        throttleFilter.Throttle(() => notifyIcon.ShowBalloonTip(minBalloonTipTimePeriodInSeconds * 1000, title, text, icon));
    }

    private void OnClickWorkItem(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem menuItem && menuItem.Tag is WorkItem workItem)
        {
            Url.Open($"{azureProject}/_workitems/edit/{workItem.Id}");
        }
        else
        {
            logger.LogError("Not a ToolStripMenuItem or not a WorkItem.");
        }
    }

    private void OnClickDashboard(object? sender, EventArgs e)
    {
        Url.Open(azureProject.ToString());
    }

    private void OnClickExit(object? sender, EventArgs e)
    {
        lifetime.StopApplication();
    }

    private static Bitmap GetWorkItemImage(string type)
    {
        return type switch
        {
            "Bug" => ImgBug,
            "Epic" => ImgEpic,
            "Feature" => ImgFeature,
            "Issue" => ImgIssue,
            "Task" => ImgTask,
            "Test Case" => ImgTestCase,
            "User Story" => ImgUserStory,
            _ => ImgSkull,
        };
    }
}
