using Microsoft.CodeAnalysis;

namespace RoslynGraph.Core;

public class TreeModel
{
    private readonly Document _document;
    public SyntaxTree? SyntaxTree { get; private set; }
    public SyntaxNode? RootNode { get; private set; }
    public SemanticModel? SemanticModel { get; private set; }

    public TreeModel(Document document)
    {
        _document = document;
    }

    public async Task LoadAsync()
    {
        if (_document == null) throw new Exception("Documento invalido.");

        SyntaxTree = await _document.GetSyntaxTreeAsync();
        RootNode = await SyntaxTree!.GetRootAsync();
        SemanticModel = await _document.GetSemanticModelAsync();
    }
}