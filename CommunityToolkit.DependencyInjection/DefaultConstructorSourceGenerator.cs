using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CommunityToolkit.DependencyInjection;

[Generator]
public class DefaultConstructorSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // TODO: Add the marker interface dynamically. This is somewhat problematic if used in multiple projects as it will produce 2 clashing types (same name and namespace in different assemblies)
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "InjectDependenciesFromDefaultConstructorAttribute.g.cs",
            SourceText.From(SourceGenerationUtil.Attribute, Encoding.UTF8)));

        IncrementalValuesProvider<DependencyInjectionTypeDeclaration?> typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        IncrementalValueProvider<(Compilation, ImmutableArray<DependencyInjectionTypeDeclaration?>)> compilationAndDecoratedTypes
            = context.CompilationProvider.Combine(typeDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndDecoratedTypes,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private static DependencyInjectionTypeDeclaration? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var syntax = (ClassDeclarationSyntax)context.Node;
        foreach (AttributeListSyntax attributeListSyntax in syntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not { } attributeTypeSymbol)
                {
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeTypeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName == SourceGenerationUtil.AttributeFullName
                    && syntax.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault(c => c.ParameterList.Parameters.Any()) is { } diConstructor)
                {
                    return new DependencyInjectionTypeDeclaration(
                        GetUsingStatements(syntax),
                        GetNamespace(syntax),
                        syntax.Modifiers.FirstOrDefault().Text,
                        syntax.Identifier.Text, diConstructor);
                }
            }
        }
        return null;
    }

    private static List<string> GetUsingStatements(ClassDeclarationSyntax syntax)
    {
        var parent = syntax.Parent;
        List<UsingDirectiveSyntax> usingDirectives = new List<UsingDirectiveSyntax>();
        while (parent is not null)
        {
            usingDirectives.AddRange(parent.ChildNodes().OfType<UsingDirectiveSyntax>());
            parent = parent.Parent;
        }
        return usingDirectives.Select(u => $"using {u.Name};").ToList();
    }

    private static string GetNamespace(ClassDeclarationSyntax syntax)
    {
        var parent = syntax.Parent;
        while (parent is not null)
        {
            if (parent is BaseNamespaceDeclarationSyntax namespaceDeclaration)
                return namespaceDeclaration.Name.ToString();
            parent = parent.Parent;
        }
        return "UnknownNamespace";
    }

    private static void Execute(Compilation compilation, ImmutableArray<DependencyInjectionTypeDeclaration?> typeDeclarations, SourceProductionContext context)
    {
        if (typeDeclarations.IsDefaultOrEmpty)
        {
            // nothing to do yet
            return;
        }
        IEnumerable<DependencyInjectionTypeDeclaration?> distinctEnums = typeDeclarations.Distinct();

        foreach (DependencyInjectionTypeDeclaration? typeDeclaration in typeDeclarations)
        {
            context.AddSource($"{typeDeclaration!.Value.ClassName}.g.cs", SourceText.From(SourceGenerationUtil.GeneratePartialClassWithDefaultCtor(typeDeclaration.Value), Encoding.UTF8));
        }
    }
}