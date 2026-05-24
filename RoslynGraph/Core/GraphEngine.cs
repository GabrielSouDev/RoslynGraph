using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using RoslynGraph.Extensions;
using RoslynGraph.Interfaces;
using RoslynGraph.Models.Graph.Edges;
using RoslynGraph.Models.Graph.Nodes;
using RoslynGraph.Utils;

namespace RoslynGraph.Core;

public class GraphEngine : IGraphEngine
{
    private readonly GraphWorkspace<Graph, DeclarationNode, InvocationEdge> _workspace;
    private Solution? _solution { get; set; }
    private readonly string _path;
    private bool _isInitialized = false;

    public GraphEngine(string path)
    {
        _path = path;
        _workspace = new GraphWorkspace<Graph, DeclarationNode, InvocationEdge>(path);
    }

    protected virtual async Task InitializeAsync()  
    {
        using var workspace = MSBuildWorkspace.Create();
        _solution = await workspace.OpenSolutionAsync(_path);
        _isInitialized = true;
    }

    private async Task EnsureInitialized()
    {
        if (!_isInitialized)
            await InitializeAsync();

        var a = await _solution!.Projects.ToList()[0].GetCompilationAsync();
    }

    public virtual async Task<Graph?> SearchInGraph(string id)
    {
        await EnsureInitialized();

        var decomposedId = id.Split('.');
        var LevelId = decomposedId.Count();

        Graph? graph = new();
        for (int i = LevelId; i >= 1; i--)
        {
            var actualId = string.Join(".", decomposedId[0..i]);
            graph = _workspace.GetById(actualId);

            if (graph != null && (!graph.Nodes.Any() || !graph.Edges.Any())) break;
        }

        return graph;
    }
    //TODO: ajustar este metodo para fazer repair. 
    //      Ideia é recriar o objeto enquanto o altera.
    public async Task SearchInSource(string id)
    {
        await EnsureInitialized();

        foreach (var project in _solution!.Projects!)
        {
            var compilation = await project.GetCompilationAsync();
            if (compilation == null) continue;

            var node = compilation.SearchNode(id);
            if (node == null) continue;

            var document = project.GetDocument(node.SyntaxTree);
            if (document == null) continue;

            var treeModel = new TreeModel(document);
            await treeModel.LoadAsync();

            //demo
            var semanticNode = GraphFactory.Build(treeModel, node);
            JsonHandler<Graph, DeclarationNode, InvocationEdge>.PrintJson(semanticNode);
        }
    }

    public async Task<Graph> FullScan()
    {
        await EnsureInitialized();

        if (_solution!.Projects == null) throw new ArgumentException("Workspace sem projetos carregados.");

        var tasks = _solution.Projects.SelectMany(p => p.Documents).Select(async document =>
        {
            var treeModel = new TreeModel(document);
            await treeModel.LoadAsync();

            return GraphFactory.Build(treeModel);
        });

        var results = await Task.WhenAll(tasks);

        var graph = new Graph
        {
            Nodes = results.SelectMany(g => g.Nodes).ToList(),
            Edges = results.SelectMany(g => g.Edges).ToList()
        };

        await _workspace.UpdateSemanticNodesAsync(graph);

        return graph;
    }
}