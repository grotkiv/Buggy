namespace Buggy;

using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

public class BuggyApplicationContext : ApplicationContext
{
    private readonly IServiceProvider services;
    private readonly BuggyNotifyIcon notifyIcon;

    public BuggyApplicationContext(IServiceProvider services)
    {
        this.services = services;
        notifyIcon = services.GetRequiredService<BuggyNotifyIcon>();
        notifyIcon.Show();
    }
}
