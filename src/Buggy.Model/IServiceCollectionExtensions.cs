namespace Buggy.Model;

using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddBuggyModel(this IServiceCollection services)
    {
        return services.AddSingleton<ObservableCollection<WorkItem>>();
    }
}