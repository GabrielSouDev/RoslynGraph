using RoslynGraph.Models.Graph.Edges;
using RoslynGraph.Models.Graph.Nodes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoslynGraph.Utils;

public class JsonHandler<TGraph, TNode, TEdge>
    where TGraph : IGraph<TNode, TEdge>, new()
    where TNode : INode, new()
    where TEdge : IEdge, new()
{
    private readonly string _path;
    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters =
            {
                new JsonStringEnumConverter()
            }
    };

    public JsonHandler(string solutionPath)
    {
        _path = solutionPath.Replace(".slnx", $"-{typeof(TGraph).Name.ToLower()}.json");
    }

    public async Task<TGraph> GetSemanticNodesAsync()
    {
        if (!File.Exists(_path))
        {
            await File.WriteAllTextAsync(_path, "[]");
            return new TGraph();
        }

        var json = await File.ReadAllTextAsync(_path);

        return JsonSerializer.Deserialize<TGraph>(json, _serializerOptions) ?? new TGraph();
    }

    public async Task UpdateSemanticNodesAsync(TGraph graph)
    {
        var json = JsonSerializer.Serialize(graph, _serializerOptions);

        await File.WriteAllTextAsync(_path, json);
    }

    public static void PrintJson(object? obj)
    {
        if (obj == null)
        {
            Console.WriteLine("Objeto Json Nulo em PrintJson.");
            return;
        }

        Console.WriteLine(JsonSerializer.Serialize(obj, _serializerOptions));
    }
}