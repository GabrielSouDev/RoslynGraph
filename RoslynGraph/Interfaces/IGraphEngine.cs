using RoslynGraph.Models.Graph.Nodes;

namespace RoslynGraph.Interfaces;

public interface IGraphEngine
{
    public Task<DeclarationNode?> Search(string id);
    public Task SearchInSource(string id);
    public Task<Graph> FullScan();
}