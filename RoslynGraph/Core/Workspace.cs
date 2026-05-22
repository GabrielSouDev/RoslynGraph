using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using RoslynGraph.Models.Graph.Edges;
using RoslynGraph.Models.Graph.Nodes;
using RoslynGraph.Utils;

namespace RoslynGraph.Core;

public class Workspace
{
    private readonly MSBuildWorkspace _workspace;
    private readonly JsonHandler _jsonHandler;
    private readonly string _path;

    public Solution? Solution { get; private set; }
    public IEnumerable<Project> Projects => Solution?.Projects ?? Enumerable.Empty<Project>();
    public Graph? Graph { get; set; }
    public Dictionary<string, List<DeclarationNode>>? Nodes { get; set; }
    public Dictionary<string, List<InvocationEdge>>? Edges { get; set; }

    public Workspace(string path)
    {
        _workspace = MSBuildWorkspace.Create();
        _path = path;
        _jsonHandler = new JsonHandler(path);
    }

    public async Task Initialize()
    {
        Solution = await _workspace.OpenSolutionAsync(_path);
        Graph = await _jsonHandler.GetSemanticNodesAsync();
        Nodes = Graph.Nodes.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.ToList());
        Edges = Graph.Edges.GroupBy(x => x.IdCaller).ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<Graph> GetSemanticNodesAsync()
    {
        return await _jsonHandler.GetSemanticNodesAsync();
    }

    public async Task UpdateSemanticNodesAsync(Graph graph)
    {
        await _jsonHandler.UpdateSemanticNodesAsync(graph);
        Graph = graph;

    }
}
