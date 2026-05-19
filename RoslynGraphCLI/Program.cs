using RoslynGraph.Core;
using RoslynGraph.Utils;

var pathArg = args.FirstOrDefault(a => a.StartsWith("-p=") || a.StartsWith("--path="));
if (string.IsNullOrEmpty(pathArg)) return;

var solutionPath = pathArg.Split('=')[1];

Console.Clear();
Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║           ROSLYN GRAPH CLI           ║");
Console.WriteLine("╠══════════════════════════════════════╣");
Console.WriteLine("║ scan           → analisar solução    ║");
Console.WriteLine("║ search         → buscar no grafo     ║");
Console.WriteLine("║ exit           → sair                ║");
Console.WriteLine("╚══════════════════════════════════════╝");

var graphEngine = new GraphEngine(solutionPath);
var input = string.Empty;
do
{
    object? result = null;
    Console.Write("Command: ");
    input = Console.ReadLine();

    switch (input!.ToLower())
    {
        case "scan":
            result = await graphEngine.FullScan();
            break;
        case "search":
            Console.Write("Search Id: ");
            input = Console.ReadLine();

            if (!string.IsNullOrEmpty(input))
                result = await graphEngine.Search(input);
            break;
        case "":
            if (Console.CursorTop > 0)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
            break;
        default:
            Console.WriteLine("Comando Invalido!");
            break;
    }

    if(result != null)
        JsonHandler.PrintJson(result);
} while (input != null && input.ToLower() != "exit");