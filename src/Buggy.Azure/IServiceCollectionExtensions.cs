namespace Buggy.Azure;

using Buggy.Model;
using Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddAzure(this IServiceCollection services)
    {
        services.AddOptions<AzureProject>()
            .BindConfiguration(nameof(AzureProject))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<IUrlProvider, AzureUrlProvider>();
        services.AddTransient<IWorkItemQuery, AzureQuery>();
        services.AddTransient<IWorkItemActions, AzureActions>();

        return services;
    }
}
