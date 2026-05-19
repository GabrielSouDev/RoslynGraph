using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynGraph.Core;

namespace RoslynGraph.Utils;
public class TreeWriter
{
    public static void PrintSolutionInfo(Solution solution)
    {
        Console.WriteLine($"Path: {solution.FilePath}");
        Console.WriteLine($"Projects: {solution.Projects.Count()}");
        Console.WriteLine();
    }

    public static void PrintProjectTree(Project project)
    {
        Console.WriteLine(project.Name);

        foreach (var document in project.Documents)
        {
            Console.WriteLine(document.Name);

            var treeModel = new TreeModel(document);

            //PrintAst(root);
            PrintMethods(treeModel);
            Console.WriteLine();
        }
    }

    private static void PrintMethods(TreeModel treeModel)
    {
        var node = treeModel.RootNode!;
        var semanticModel = treeModel.SemanticModel!;

        var methods = node.DescendantNodes()
                          .OfType<MethodDeclarationSyntax>();

        foreach (var method in methods)
        {
            // ✅ símbolo do método
            var methodDeclaredSymbol = semanticModel.GetDeclaredSymbol(method);
            if (methodDeclaredSymbol == null)
                continue;

            var declaringType = methodDeclaredSymbol.ContainingType;
            Console.WriteLine(methodDeclaredSymbol.Name);
            PrintUsedImplementations(method, semanticModel);
            Console.WriteLine("   Return Type: " + methodDeclaredSymbol.ReturnType.ToDisplayString());

            Console.WriteLine("   Parameters:");
            foreach (var p in methodDeclaredSymbol.Parameters)
            {
                Console.WriteLine($"      {p.Type.ToDisplayString()} {p.Name}");
            }

            Console.WriteLine();
        }
    }

    private static void PrintUsedImplementations(SyntaxNode root, SemanticModel semanticModel)
    {
        var invocations = root.DescendantNodes()
                              .OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(invocation);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

            if (methodSymbol == null)
                continue;

            Console.WriteLine($"   Método chamado: {methodSymbol.ToDisplayString()}");
            Console.WriteLine("   Classe: " + methodSymbol.ContainingType.ToDisplayString());
            Console.WriteLine();
        }
    }


    private static void PrintAst(SyntaxNode node)
    {
        PrintAstFull(node, 0);
    }

    private static void PrintAstFull(SyntaxNode node, int indent = 0)
    {
        Console.WriteLine($"{new string(' ', indent * 2)}- {node.Kind()}");

        foreach (var child in node.ChildNodesAndTokens())
        {
            if (child.IsNode)
            {
                PrintAstFull(child.AsNode()!, indent + 1);
            }
            else
            {
                var token = child.AsToken();
                Console.WriteLine(
                    $"{new string(' ', (indent + 1) * 2)}* {token.Kind()} : '{token.Text}'");
            }
        }
    }
}

//var code = "Console.WriteLine(\"Hello, World!\");";

//var tree = CSharpSyntaxTree.ParseText(code);

//var root = tree.GetRoot();

//PrintAst(root);