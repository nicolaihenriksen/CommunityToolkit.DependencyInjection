using Microsoft.Extensions.DependencyInjection;

namespace CommunityToolkit.DependencyInjection;

public class ServiceProviderAccessor
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    internal static void Initialize(IServiceProvider serviceProvider, SourceGeneratorOptions options)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// This would be used if the <see cref="HostBuilderExtensions.UseSourceGeneratedDefaultConstructors"/> worked correctly
    /// </summary>
    public ServiceProviderAccessor(IServiceProvider serviceProvider, SourceGeneratorOptions options)
    {
        Initialize(serviceProvider, options);
    }

    public static T Resolve<T>() where T : notnull
    {
        if (ServiceProvider is null)
            throw new InvalidOperationException("HostBuilder is not configured to use the 'CommunityToolkit.DependencyInjection' package. You need to call UseSourceGeneratedDefaultConstructors() on the host builder");

        return ServiceProvider.GetRequiredService<T>();
    }
}