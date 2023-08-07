using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CommunityToolkit.DependencyInjection.SourceGenerators;

/// <summary>
/// Represents a type for which we should source-generate a default constructor and inject the dependencies
/// required by the constructor identified by the <see cref="DependencyInjectionConstructor"/> property.
///
/// This must contain all information needed to generate a partial class with the default constructor.
/// </summary>
internal record struct DependencyInjectionTypeDeclaration(
    List<string> UsingStatements,
    string Namespace,
    string AccessModifier,
    string ClassName,
    ConstructorDeclarationSyntax DependencyInjectionConstructor);