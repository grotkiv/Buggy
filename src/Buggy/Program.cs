namespace Buggy;

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Buggy.Azure;
using Buggy.Model;

public static class Program
{
    [STAThread]
    public static async Task Main(string [] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services))
            .ConfigureServices(services =>
            {
                services.AddAzure();
                services.AddSingleton<BuggyNotifyIcon>();
                services.AddBuggyModel();
            })
            .Build();

        var hostTask = host.RunAsync();

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // There must be no winforms element created before this line.
        // That's why we pass host.Services to our application context object.
        // The ctor is creating the winforms components using GetRequiredService().
        Application.Run(new BuggyApplicationContext(host.Services));

        await hostTask;
    }
}

