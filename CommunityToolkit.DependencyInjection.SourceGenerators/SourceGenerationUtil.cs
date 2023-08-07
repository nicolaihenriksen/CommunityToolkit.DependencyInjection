namespace CommunityToolkit.DependencyInjection.SourceGenerators;

internal static class SourceGenerationUtil
{
    private const string Newline = "\r\n";

    private static readonly List<string> RequiredUsingStatements = new()
    {
        "using System.Xaml;",
        "using Microsoft.Extensions.DependencyInjection;"
    };

    public static string Namespace => typeof(SourceGenerationUtil).Namespace!;

    public const string AttributeName = "InjectDependenciesFromDefaultConstructorAttribute";

    public static string AttributeFullName => $"{Namespace}.{AttributeName}";

    public static string Attribute = $$"""
namespace {{Namespace}};

[System.AttributeUsage(System.AttributeTargets.Class)]
public class {{AttributeName}} : System.Attribute
{
}
""";


    internal static string GeneratePartialClassWithDefaultCtor(DependencyInjectionTypeDeclaration typeDeclaration)
    {
        return $$"""
{{GetRequiredUsingStatements(typeDeclaration)}}

namespace {{typeDeclaration.Namespace}};

{{typeDeclaration.AccessModifier}} partial class {{typeDeclaration.ClassName}}
{
    public {{typeDeclaration.ClassName}}() :
        this({{GetResolvedDependencies(typeDeclaration)}}) { }

    private static T __Resolve<T>() where T : notnull
    {
        if (CommunityToolkit.DependencyInjection.ServiceProviderAccessor.ServiceProvider is null)
            throw new XamlParseException("HostBuilder is not configured to use the 'CommunityToolkit.DependencyInjection' package. You need to call UseSourceGeneratedDefaultConstructors() on the host builder");

        try
        {
            return CommunityToolkit.DependencyInjection.ServiceProviderAccessor.ServiceProvider.GetRequiredService<T>();
        }
        catch
        {
            throw new XamlParseException($"Unable to find required service of type '{typeof(T).Name}' for ctor for type '{nameof({{typeDeclaration.ClassName}})}'");
        }
    }
}
""";
    }

    private static string GetRequiredUsingStatements(DependencyInjectionTypeDeclaration typeDeclaration)
        => string.Join(Newline, RequiredUsingStatements.Concat(typeDeclaration.UsingStatements).Except(new[] { $"using {Namespace};" }).Distinct());

    private static string GetResolvedDependencies(DependencyInjectionTypeDeclaration typeDeclaration)
        => string.Join(", ", typeDeclaration.DependencyInjectionConstructor.ParameterList.Parameters.Select(p => $"__Resolve<{p.Type}>()"));
}