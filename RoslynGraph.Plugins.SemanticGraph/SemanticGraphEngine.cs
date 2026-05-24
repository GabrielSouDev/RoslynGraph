using Microsoft.CodeAnalysis;
using RoslynGraph.Models.Graph.Edges;
using RoslynGraph.Models.Graph.Nodes;
using System.Threading.Channels;

namespace RoslynGraph.Core;

public class SemanticGraphEngine : GraphEngine
{

    private readonly GraphWorkspace<SemanticGraph, SemanticDeclarationNode, SemanticInvocationEdge> _semanticWorkspace;

    private readonly Channel<DeclarationNode> _nodeChannel;
    private readonly Channel<InvocationEdge> _edgeChannel;

    public SemanticGraphEngine(string path) : base(path) 
    {
        _semanticWorkspace = new GraphWorkspace<SemanticGraph, SemanticDeclarationNode, SemanticInvocationEdge>(path);
        _nodeChannel = Channel.CreateUnbounded<DeclarationNode>();
        _edgeChannel = Channel.CreateUnbounded<InvocationEdge>();
    }

    protected override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _ = Task.Run(() => ListeningNodes(_nodeChannel));
        _ = Task.Run(() => ListeningEdges(_edgeChannel));

    }

    private async Task ListeningNodes(Channel<DeclarationNode> channel)
    {
        await foreach (var node in channel.Reader.ReadAllAsync())
        {
            throw new NotImplementedException();
        }
    }

    private async Task ListeningEdges(Channel<InvocationEdge> channel)
    {
        await foreach (var edge in channel.Reader.ReadAllAsync())
        {
            throw new NotImplementedException();
        }
    }

    public override async Task<Graph?> SearchInGraph(string id)
    {
        var graph = await base.SearchInGraph(id);

        return graph;
    }
}

public class SemanticGraph : IGraph<SemanticDeclarationNode, SemanticInvocationEdge>
{
    public List<SemanticDeclarationNode> Nodes { get; set; } = new List<SemanticDeclarationNode>();
    public List<SemanticInvocationEdge> Edges { get; set; } = new List<SemanticInvocationEdge>();

}