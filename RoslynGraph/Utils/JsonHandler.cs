using RoslynGraph.Models.Graph.Nodes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoslynGraph.Utils;
public class JsonHandler
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
        _path = solutionPath.Replace(".slnx", "-graph.json");
    }

    public async Task<List<DeclarationNode>> GetSemanticNodesAsync()
    {
        if (!File.Exists(_path))
        {
            await File.WriteAllTextAsync(_path, "[]");
            return new List<DeclarationNode>();
        }

        var json = await File.ReadAllTextAsync(_path);

        return JsonSerializer.Deserialize<List<DeclarationNode>>(json, _serializerOptions) ?? new();
    }

    public async Task UpdateSemanticNodesAsync(IEnumerable<DeclarationNode> semanticNodes)
    {
        var json = JsonSerializer.Serialize(semanticNodes, _serializerOptions);

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