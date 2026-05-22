using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynGraph.Core;
using RoslynGraph.Models.Graph.Edges;
using RoslynGraph.Models.Graph.Nodes;

namespace RoslynGraph.Extensions;

public static class SemanticNodeFactory
{
    public static Graph Build(TreeModel treeModel)
    {
        var typeDeclarationsSyntax = treeModel.GetTypeDeclarations();

        var methodsSyntax = typeDeclarationsSyntax
            .SelectMany(d => d.GetMethodsDeclaration());

        var invocationExpressions = methodsSyntax
            .SelectMany(m => m.GetInvocationsExpression());

        var declarations = typeDeclarationsSyntax
            .Select(treeModel.BuildDeclarations)
            .Where(semanticNode => semanticNode != null)
            .ToList();


        var methods = methodsSyntax
            .Select(treeModel.BuildMethod)
            .Where((methodNode) => methodNode.Item1 != null);


        var result = new Graph();
        result.Nodes.AddRange(declarations!);
        result.Nodes.AddRange(methods.Select(m => m.Item1)!);
        result.Edges.AddRange(methods.SelectMany(m => m.Item2!)!);

        return result;
    }

    public static Graph Build(TreeModel treeModel, SyntaxNode node)
    {
        var typeDeclarationsSyntax = node.GetTypeDeclarations();

        var methodsSyntax = typeDeclarationsSyntax
            .SelectMany(d => d.GetMethodsDeclaration());

        var invocationExpressions = methodsSyntax
            .SelectMany(m => m.GetInvocationsExpression());

        var declarations = typeDeclarationsSyntax
            .Select(treeModel.BuildDeclarations)
            .Where(semanticNode => semanticNode != null)
            .ToList();


        var methods = methodsSyntax
            .Select(treeModel.BuildMethod)
            .Where((methodNode) => methodNode.Item1 != null);


        var result = new Graph();
        result.Nodes.AddRange(declarations!);
        result.Nodes.AddRange(methods.Select(m => m.Item1)!);
        result.Edges.AddRange(methods.SelectMany(m => m.Item2!)!);

        return result;
    }

    private static InvocationEdge? BuildInvocation(this TreeModel treeModel, string methodId, InvocationExpressionSyntax invocation)
    {
        var invocationId = treeModel.GetInvocationId(invocation);
        if (string.IsNullOrEmpty(invocationId))
            return null;

        bool isExternal = treeModel.InvocationIsExternal(invocation);

        return new InvocationEdge()
        {
            IdCaller = methodId,
            IdCalled = invocationId,
            IsExternal = isExternal
        };
    }

    private static (MethodDeclarationNode?, IEnumerable<InvocationEdge>?) BuildMethod(this TreeModel treeModel, MethodDeclarationSyntax methodDeclaration)
    {
        Dictionary<string, MethodDeclarationNode> result = new();
        var methodSymbol = treeModel.GetSymbol(methodDeclaration);
        if (methodSymbol == null) return (null, null);

        var methodId = methodSymbol.GetSymbolId();
        if (string.IsNullOrEmpty(methodId)) return (null, null);

        var body = methodDeclaration.GetMethodBody();
        var acessModifier = methodSymbol.GetAccessModifier();
        var name = methodSymbol.GetName();
        var parameters = treeModel.GetParameters(methodDeclaration.ParameterList);
        var returnType = methodDeclaration.GetReturnType();

        var methodNode = new MethodDeclarationNode()
        {
            Id = methodId,
            AccessModifier = acessModifier,
            Name = name,
            Parameters = parameters,
            ReturnType = returnType,
            Body = body
        };

        var invocationsEdges = new List<InvocationEdge>();
        var invocations = methodDeclaration.GetInvocationsExpression();
        foreach (var invocation in invocations)
        {
            var invocationEdge = treeModel.BuildInvocation(methodId, invocation);
            if (invocationEdge == null)
                continue;

            invocationsEdges.Add(invocationEdge);
        }
        return (methodNode, invocationsEdges);
    }

    private static TypeDeclarationNode? BuildDeclarations(this TreeModel treeModel, TypeDeclarationSyntax declaration)
    {
        var declarationSymbol = treeModel.GetSymbol(declaration);
        if (declarationSymbol == null) return null;

        var declarationId = declarationSymbol.GetSymbolId();
        if (string.IsNullOrEmpty(declarationId))
            return null;

        var category = treeModel.Classify(declaration);
        var acessModifier = declarationSymbol.GetAccessModifier();
        var name = declarationSymbol.GetName();

        TypeDeclarationNode? semanticNode = declaration switch
        {
            ClassDeclarationSyntax => new ClassDeclarationNode(),
            InterfaceDeclarationSyntax => new InterfaceDeclarationNode(),
            StructDeclarationSyntax => new StructDeclarationNode(),
            RecordDeclarationSyntax => new RecordDeclarationNode(),
            _ => null
        };
        if (semanticNode == null) return null;

        semanticNode.Id = declarationId;
        semanticNode.Category = category;
        semanticNode.AccessModifier = acessModifier;
        semanticNode.Name = name;

        return semanticNode;
    }
}