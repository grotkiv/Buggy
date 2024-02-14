namespace Buggy;

using System;

public class ThrottleFilter
{
    private readonly TimeSpan window;
    private DateTime lastRun;

    public ThrottleFilter()
        : this(TimeSpan.FromMilliseconds(500))
    {
    }

    public ThrottleFilter(TimeSpan window)
    {
        this.window = window;
        lastRun = DateTime.MinValue;
    }

    public void Throttle(Action action)
    {
        var now = DateTime.Now;
        var delta = now - lastRun;
        if (delta > window)
        {
            action.Invoke();
            lastRun = now;
        }
    }
}
