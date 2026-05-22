using RoslynGraph.Extensions;
using RoslynGraph.Interfaces;
using RoslynGraph.Models.Graph.Nodes;
using RoslynGraph.Utils;

namespace RoslynGraph.Core;

public class GraphEngine : IGraphEngine
{
    protected readonly Workspace _workspace;
    private bool _isInitialized = false;

    public GraphEngine(string path)
    {
        _workspace = new Workspace(path);
    }

    protected virtual async Task InitializeAsync()
    {
        await _workspace.Initialize();
        _isInitialized = true;
    }

    private async Task EnsureInitialized()
    {
        if (!_isInitialized)
            await InitializeAsync();

        var a = await _workspace.Projects.ToList()[0].GetCompilationAsync();
    }

    //ALTERAR TUDO DO SEARCH
    public async Task<Graph?> Search(string id)
    {
        await EnsureInitialized();

        var decomposedId = id.Split('.');
        var LevelId = decomposedId.Count();

        Graph graph = new();
        for (int i = LevelId; i >= 1; i--)
        {
            _workspace.Nodes!.TryGetValue(string.Join(".", decomposedId[0..i]), out var nodes);
            _workspace.Edges!.TryGetValue(string.Join(".", decomposedId[0..i]), out var edges);

            if(nodes != null)
                graph.Nodes = nodes;

            if (edges != null)
                graph.Edges = edges;

            if (!graph.Nodes.Any() || !graph.Edges.Any()) break;
        }

        return graph;
    }
    //TODO: ajustar este metodo para fazer repair. 
    //      Ideia é recriar o objeto enquanto o altera.
    public async Task SearchInSource(string id)
    {
        await EnsureInitialized();

        foreach (var project in _workspace.Projects!)
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
            var semanticNode = SemanticNodeFactory.Build(treeModel, node);
            JsonHandler.PrintJson(semanticNode);
        }
    }

    public async Task<Graph> FullScan()
    {
        await EnsureInitialized();

        if (_workspace.Projects == null) throw new ArgumentException("Workspace sem projetos carregados.");

        var tasks = _workspace.Projects.SelectMany(p => p.Documents).Select(async document =>
        {
            var treeModel = new TreeModel(document);
            await treeModel.LoadAsync();

            return SemanticNodeFactory.Build(treeModel);
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