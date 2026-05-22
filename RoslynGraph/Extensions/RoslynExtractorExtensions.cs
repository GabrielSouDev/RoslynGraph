using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynGraph.Core;
using RoslynGraph.Models.Enums;
using RoslynGraph.Models.Graph.Nodes;

namespace RoslynGraph.Extensions;

public static class RoslynExtractorExtensions
{
    private static CategoryType Classify(INamedTypeSymbol symbol)
    {
        var ns = symbol.ContainingNamespace?.ToString() ?? "";
        var name = symbol.Name;
        var baseType = symbol.BaseType?.Name ?? "";

        return (name, ns, baseType) switch
        {
            // 🟣 DOMAIN
            (var n, _, _) when n.EndsWith("Entity") => CategoryType.Domain,
            (var n, _, _) when n.EndsWith("ValueObject") => CategoryType.Domain,
            (_, var n, _) when n.Contains("Domain") => CategoryType.Domain,
            (_, _, "Entity") => CategoryType.Domain,

            // 🟡 APPLICATION
            (var n, _, _) when n.EndsWith("Service") => CategoryType.Application,
            (var n, _, _) when n.EndsWith("Handler") => CategoryType.Application,
            (var n, _, _) when n.EndsWith("UseCase") => CategoryType.Application,
            (_, var n, _) when n.Contains("Application") => CategoryType.Application,

            // 🔵 INFRASTRUCTURE
            (var n, _, _) when n.EndsWith("Repository") => CategoryType.Infrastructure,
            (var n, _, _) when n.Contains("Db") => CategoryType.Infrastructure,
            (var n, _, _) when n.Contains("Client") => CategoryType.Infrastructure,
            (var n, _, _) when n.Contains("Http") => CategoryType.Infrastructure,
            (var n, _, _) when n.Contains("Adapter") => CategoryType.Infrastructure,
            (_, var n, _) when n.Contains("Infrastructure") => CategoryType.Infrastructure,
            (_, _, var b) when b.Contains("DbContext") => CategoryType.Infrastructure,

            // 🟢 PRESENTATION
            (var n, _, _) when n.EndsWith("Controller") => CategoryType.Presentation,
            (var n, _, _) when n.EndsWith("ViewModel") => CategoryType.Presentation,
            (var n, _, _) when n.EndsWith("Dto") => CategoryType.Presentation,
            (var n, _, _) when n.Contains("Page") => CategoryType.Presentation,
            (_, var n, _) when n.Contains("Api") => CategoryType.Presentation,

            // ⚪ SHARED
            (var n, _, _) when n.Contains("Helper") => CategoryType.Shared,
            (var n, _, _) when n.Contains("Utils") => CategoryType.Shared,
            (var n, _, _) when n.Contains("Common") => CategoryType.Shared,
            (_, var n, _) when n.Contains("Shared") => CategoryType.Shared,

            // 🔴 FALLBACK
            _ => CategoryType.Unknown
        };
    }

    public static IEnumerable<TypeDeclarationSyntax> GetTypeDeclarations(this TreeModel treeModel)
    {
        return treeModel.RootNode!.DescendantNodes().OfType<TypeDeclarationSyntax>();
    }

    public static IEnumerable<TypeDeclarationSyntax> GetTypeDeclarations(this SyntaxNode node)
    {
        var declaration = node.FirstAncestorOrSelf<TypeDeclarationSyntax>();

        if (declaration != null)
        {
            yield return declaration;
            yield break;
        }

        foreach (var t in node.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            yield return t;
        }
    }

    public static string GetSymbolId(this ISymbol symbol)
    {
        return symbol.ToDisplayString(SymbolFormat.Id);
    }

    public static CategoryType Classify(this TreeModel treeModel, TypeDeclarationSyntax typeDeclaration)
    {
        var namedSymbol = treeModel.SemanticModel!.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;

        if (namedSymbol == null) return CategoryType.Unknown;

        return Classify(namedSymbol);
    }

    public static IEnumerable<MethodDeclarationSyntax> GetMethodsDeclaration(this TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();
    }

    public static IEnumerable<InvocationExpressionSyntax> GetInvocationsExpression(this MethodDeclarationSyntax methodDeclaration)
    {
        return methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();
    }

    public static string GetMethodBody(this MethodDeclarationSyntax methodDeclaration)
    {
        return methodDeclaration.Body?.ToString()
                ?? methodDeclaration.ExpressionBody?.Expression.ToString()
                ?? string.Empty;
    }

    public static Accessibility GetAccessModifier(this ISymbol symbol)
    {
        return symbol?.DeclaredAccessibility ?? Accessibility.NotApplicable;
    }

    public static string GetName(this ISymbol symbol)
    {
        return symbol?.Name ?? string.Empty;
    }

    public static ISymbol? GetSymbol(this TreeModel treeModel, SyntaxNode node)
    {
        return treeModel.SemanticModel!.GetDeclaredSymbol(node);
    }
    public static IEnumerable<ParameterDeclarationNode> GetParameters(this TreeModel treeModel, ParameterListSyntax parameterList)
    {
        foreach (var parameter in parameterList.Parameters)
        {
            var typeInfo = treeModel.SemanticModel.GetTypeInfo(parameter.Type!);
            var typeSymbol = typeInfo.Type;

            var name = parameter.Identifier.Text;
            var typeName = typeSymbol?.ToDisplayString(SymbolFormat.Id);

            if (typeName == null) continue;

            yield return new ParameterDeclarationNode
            {
                Name = name,
                Type = typeName
            };
        }
    }

    public static string GetReturnType(this MethodDeclarationSyntax methodDeclaration)
    {
        return methodDeclaration.ReturnType?.ToString() ?? string.Empty;
    }

    public static string GetInvocationId(this TreeModel treeModel, InvocationExpressionSyntax invocation)
    {
        var symbol = treeModel.SemanticModel!.GetSymbolInfo(invocation).Symbol;

        if (symbol == null)
            return string.Empty;

        return symbol.ToDisplayString(SymbolFormat.Id);
    }

    public static bool InvocationIsExternal(this TreeModel treeModel, InvocationExpressionSyntax invocation)
    {
        var symbol = treeModel.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (symbol == null)
            return false;

        var currentAssembly = treeModel.SemanticModel!.Compilation.Assembly;

        return !SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, currentAssembly);
    }
}

public static class SymbolFormat
{
    public static SymbolDisplayFormat Id =>
        new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions: SymbolDisplayMemberOptions.IncludeContainingType,
            parameterOptions: SymbolDisplayParameterOptions.IncludeType,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                SymbolDisplayMiscellaneousOptions.ExpandNullable
        );
}

public static class RoslynNavigationExtensions
{
    public static SyntaxNode? SearchNode(this Compilation compilation, string id)
    {
        var Symbol = compilation.GetSymbol(id);

        if (Symbol == null) return null;
        var syntaxRef = Symbol.DeclaringSyntaxReferences.FirstOrDefault();

        if (syntaxRef == null) return null;

        return syntaxRef.GetSyntax();
    }

    private static ISymbol? GetSymbol(this Compilation compilation, string id)
    {
        var symbols = compilation.GlobalNamespace
            .GetMembers()
            .SelectMany(m => GetAllSymbols(m));

        var match = symbols.FirstOrDefault(s =>
            s.ToDisplayString(SymbolFormat.Id) == id);

        return match ?? null;
    }

    private static IEnumerable<ISymbol> GetAllSymbols(ISymbol symbol)
    {
        yield return symbol;

        switch (symbol)
        {
            case INamespaceSymbol ns:
                foreach (var member in ns.GetMembers())
                    foreach (var child in GetAllSymbols(member))
                        yield return child;
                break;

            case INamedTypeSymbol type:
                foreach (var member in type.GetMembers())
                    foreach (var child in GetAllSymbols(member))
                        yield return child;
                break;
        }
    }
}

public class NavigationResult
{
    public required DeclarationNode Main { get; set; }
    public IEnumerable<DeclarationNode> Dependences { get; set; } = Enumerable.Empty<DeclarationNode>();
}