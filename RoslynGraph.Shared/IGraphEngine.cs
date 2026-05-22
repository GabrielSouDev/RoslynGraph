using RoslynGraph.Shared.Models.Graph.Nodes;

namespace RoslynGraph.Shared;

public interface IGraphEngine
{
    public Task<DeclarationNode?> Search(string id);
    public Task SearchInSource(string id);
    public Task<Graph> FullScan();
}