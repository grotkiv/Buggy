namespace Buggy.Model;

using System;
using CommunityToolkit.Mvvm.ComponentModel;

public class WorkItem : ObservableObject
{
    private long id;
    private string title;
    private string type;
    private string state;

    public WorkItem(long id, string title, string type, string state)
    {
        ArgumentNullException.ThrowIfNull(title);
        this.id = id;
        this.title = title;
        this.type = type;
        this.state = state;
    }

    public long Id
    {
        get { return id; }
        set { SetProperty(ref id, value); }
    }

    public string Title
    {
        get { return title; }
        set { SetProperty(ref title, value); }
    }

    public string Type
    {
        get { return type; }
        set { SetProperty(ref type, value); }
    }

    public string State
    {
        get { return state; }
        set { SetProperty(ref state, value); }
    }

    public override bool Equals(object? obj)
    {
        return obj is WorkItem wi &&
            Id == wi.id &&
            Title.Equals(wi.Title) &&
            Type.Equals(wi.Type) &&
            State.Equals(wi.State);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Title, Type, State);
    }
}