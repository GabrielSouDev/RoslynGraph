namespace RoslynGraph.Models.Graph.Edges;

public class InvocationEdge
{
    public string IdCaller { get; set; } = string.Empty;
    public string IdCalled { get; set; } = string.Empty;
    public bool IsExternal { get; set; }
}
