using RoslynGraph.Models.Graph.Edges;

namespace RoslynGraph.Models.Graph.Nodes;

public class Graph
{
    public List<DeclarationNode> Nodes { get; set; } = new List<DeclarationNode>();
    public List<InvocationEdge> Edges { get; set; } = new List<InvocationEdge>();
}