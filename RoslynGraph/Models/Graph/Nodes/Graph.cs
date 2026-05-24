using RoslynGraph.Models.Graph.Edges;

namespace RoslynGraph.Models.Graph.Nodes;

public interface IGraph<TNode, TEdge>
    where TNode : INode
    where TEdge : IEdge
{
    List<TNode> Nodes { get; set; }
    List<TEdge> Edges { get; set; }
}

public class Graph : IGraph<DeclarationNode, InvocationEdge>
{
    public List<DeclarationNode> Nodes { get; set; } = new List<DeclarationNode>();
    public List<InvocationEdge> Edges { get; set; } = new List<InvocationEdge>();
}