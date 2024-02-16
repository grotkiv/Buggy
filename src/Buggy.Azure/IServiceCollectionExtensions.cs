namespace Buggy.Azure;

using Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddAzure(this IServiceCollection services)
    {
        services.AddOptions<AzureProject>()
            .BindConfiguration(nameof(AzureProject))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHostedService<QueryExecutor>();

        return services;
    }
}
