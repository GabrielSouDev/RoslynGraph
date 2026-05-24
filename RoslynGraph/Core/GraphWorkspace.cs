using Microsoft.CodeAnalysis;
using RoslynGraph.Models.Graph.Edges;
using RoslynGraph.Models.Graph.Nodes;
using RoslynGraph.Utils;

namespace RoslynGraph.Core;

public class GraphWorkspace<TGraph, TNode, TEdge>
    where TGraph : IGraph<TNode, TEdge>, new()
    where TNode : INode, new()
    where TEdge : IEdge, new()
{
    private readonly JsonHandler<TGraph, TNode, TEdge> _jsonHandler;
    private readonly string _path;
    public TGraph? Graph { get; set; }
    public Dictionary<string, List<TNode>>? Nodes { get; set; }
    public Dictionary<string, List<TEdge>>? Edges { get; set; }

    public GraphWorkspace(string path)
    {
        _path = path;
        _jsonHandler = new JsonHandler<TGraph, TNode, TEdge>(path);
    }

    public async Task Initialize()
    {
        Graph = await _jsonHandler.GetSemanticNodesAsync();
        Nodes = Graph.Nodes.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.ToList());
        Edges = Graph.Edges.GroupBy(x => x.IdStart).ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<TGraph> GetSemanticNodesAsync()
    {
        return await _jsonHandler.GetSemanticNodesAsync();
    }

    public async Task UpdateSemanticNodesAsync(TGraph graph)
    {
        await _jsonHandler.UpdateSemanticNodesAsync(graph);
        Graph = graph;
    }

    public TGraph? GetById(string id)
    {
        Nodes!.TryGetValue(id, out var nodes);
        Edges!.TryGetValue(id, out var edges);

        if (nodes == null && edges == null) return default;

        TGraph graph = new();
        if (nodes != null)
            graph.Nodes = nodes;

        if (edges != null)
            graph.Edges = edges;

        return graph;
    }
}
