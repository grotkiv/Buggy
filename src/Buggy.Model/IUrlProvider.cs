namespace Buggy.Model;

using System;

public interface IUrlProvider
{
    string GetProjectOverviewUrl();

    string GetWorkItemUrl(WorkItem workItem);
}