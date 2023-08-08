namespace CommunityToolkit.DependencyInjection.SourceGenerators;

internal static class SourceGenerationUtil
{
    private const string Newline = "\r\n";

    internal static string GeneratePartialClassWithDefaultCtor(DependencyInjectionTypeDeclaration typeDeclaration)
    {
        return $$"""
{{GetRequiredUsingStatements(typeDeclaration)}}

namespace {{typeDeclaration.Namespace}}
{
    {{typeDeclaration.AccessModifier}} partial class {{typeDeclaration.ClassName}}
    {
        public {{typeDeclaration.ClassName}}() : this({{GetResolvedDependencies(typeDeclaration)}})
        { }
    }
}
""";
    }

    private static string GetRequiredUsingStatements(DependencyInjectionTypeDeclaration typeDeclaration)
        => string.Join(Newline, typeDeclaration.UsingStatements.Except(new[] { $"using CommunityToolkit.DependencyInjection;" }));

    private static string GetResolvedDependencies(DependencyInjectionTypeDeclaration typeDeclaration)
        => string.Join(", ", typeDeclaration.DependencyInjectionConstructor.ParameterList.Parameters.Select(p => $"CommunityToolkit.DependencyInjection.ServiceProviderAccessor.Resolve<{p.Type}>()"));
}