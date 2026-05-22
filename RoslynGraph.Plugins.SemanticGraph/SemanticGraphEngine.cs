using Microsoft.CodeAnalysis;
using RoslynGraph.Models.Graph.Nodes;
using System.Threading.Channels;

namespace RoslynGraph.Core;

public class SemanticGraphEngine : GraphEngine
{
    private SemanticGraph? _semanticGraph = new SemanticGraph();

    private readonly Channel<DeclarationNode> _channel;
    public SemanticGraphEngine(string path) : base(path) 
    {
        _channel = Channel.CreateUnbounded<DeclarationNode>();
    }

    protected override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _ = Task.Run(() => ListeningGraph(_channel));

        _semanticGraph = _workspace.Graph as SemanticGraph;
    }

    private async Task ListeningGraph(Channel<DeclarationNode> channel)
    {
        await foreach (var node in channel.Reader.ReadAllAsync())
        {
            throw new NotImplementedException();
        }
    }
}

public class SemanticGraph : Graph
{
    public List<DeclarationNode> Nodes { get; set; }
    public List<DeclarationNode> Edges { get; set; }

}