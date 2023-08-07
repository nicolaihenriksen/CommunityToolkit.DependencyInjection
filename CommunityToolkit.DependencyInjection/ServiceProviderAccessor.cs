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
}