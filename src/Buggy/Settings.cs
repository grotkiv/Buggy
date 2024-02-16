namespace Buggy;

using System;

public class Settings
{
    public TimeSpan UpdatePeriod { get; set; } = TimeSpan.FromSeconds(10);
}