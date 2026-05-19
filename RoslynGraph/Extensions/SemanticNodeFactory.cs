using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynGraph.Core;
using RoslynGraph.Models.Graph.Edges;
using RoslynGraph.Models.Graph.Nodes;
using System.Xml.Linq;

namespace RoslynGraph.Extensions;
public static class SemanticNodeFactory
{
    public static IEnumerable<DeclarationNode> Build(TreeModel treeModel)
    {
        var result = new List<DeclarationNode>();
        var typeDeclarations = treeModel.GetTypeDeclarations();
        foreach (var declaration in typeDeclarations)
        {
            var semanticNode = treeModel.BuildDeclarations(declaration);

            if (semanticNode == null)
                continue;

            result.Add(semanticNode);
        }
        return result;
    }

    public static IEnumerable<DeclarationNode> Build(TreeModel treeModel, SyntaxNode node)
    {
        var result = new List<DeclarationNode>();
        var typeDeclarations = node.GetTypeDeclarations();
        foreach (var declaration in typeDeclarations)
        {
            var semanticNode = treeModel.BuildDeclarations(declaration);

            if (semanticNode == null)
                continue;

            result.Add(semanticNode);
        }
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

    private static MethodDeclarationNode? BuildMethod(this TreeModel treeModel, MethodDeclarationSyntax methodDeclaration)
    {
        var methodSymbol = treeModel.GetSymbol(methodDeclaration);
        if (methodSymbol == null) return null;

        var methodId = methodSymbol.GetSymbolId();
        if (string.IsNullOrEmpty(methodId)) return null;

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

        var invocations = methodDeclaration.GetInvocationsExpression();
        foreach (var invocation in invocations)
        {
            var invocationEdge = treeModel.BuildInvocation(methodId, invocation);
            if (invocationEdge == null)
                continue;

            methodNode.Invocations.Add(invocationEdge);
        }
        return methodNode;
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

        var methodsDeclarations = declaration.GetMethodsDeclaration();
        foreach (var methodDeclaration in methodsDeclarations)
        {
            var methodNode = treeModel.BuildMethod(methodDeclaration);
            if (methodNode == null)
                continue;

            semanticNode.Methods.Add(methodNode);
        }
        return semanticNode;
    }
}