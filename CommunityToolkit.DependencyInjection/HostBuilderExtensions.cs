using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommunityToolkit.DependencyInjection;

public static class HostBuilderExtensions
{
    /// <summary>
    /// This is ideally how I would like it to work because it follows the normal generic host approach,
    /// but I cannot seem to get the instance XXX to be instantiated when the container is built. Something
    /// needs to request the service before that happens :-(
    /// </summary>
    public static IHostBuilder UseSourceGeneratedDefaultConstructors(this IHostBuilder hostBuilder, Action<SourceGeneratorOptions>? configuration = null)
    {
        SourceGeneratorOptions options = new();
        configuration?.Invoke(options);
        hostBuilder.ConfigureServices((_, services) =>
        {
            services.AddSingleton(serviceProvider => new ServiceProviderAccessor(serviceProvider, options));
        });
        return hostBuilder;
    }

    /// <summary>
    /// Workaround because I cannot get the extension method above to work the way I desire.
    /// </summary>
    public static IHost BuildWithSourceGeneratedDefaultConstructors(this IHostBuilder hostBuilder, Action<SourceGeneratorOptions>? configuration = null)
    {
        SourceGeneratorOptions options = new();
        configuration?.Invoke(options);
        var host = hostBuilder.Build();
        ServiceProviderAccessor.Initialize(host.Services, options);
        return host;
    }
}