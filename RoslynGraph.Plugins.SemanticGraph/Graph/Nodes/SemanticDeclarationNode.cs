using Microsoft.CodeAnalysis;
using RoslynGraph.Models.Enums;
using System.Text.Json.Serialization;

namespace RoslynGraph.Models.Graph.Nodes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "nodeType")]
[JsonDerivedType(typeof(SemanticClassDeclarationNode), "semantic-class")]
[JsonDerivedType(typeof(SemanticInterfaceDeclarationNode), "semantic-interface")]
[JsonDerivedType(typeof(SemanticMethodDeclarationNode), "semantic-method")]
[JsonDerivedType(typeof(SemanticRecordDeclarationNode), "semantic-record")]
[JsonDerivedType(typeof(SemanticStructDeclarationNode), "semantic-struct")]
public class SemanticDeclarationNode : INode
{
    public string Id { get; set; } = string.Empty;
    public Guid IdStable { get; init; } = Guid.NewGuid();
}

public abstract class SemanticTypeDeclarationNode : DeclarationNode
{
    public string Name { get; set; } = string.Empty;

    public Accessibility AccessModifier { get; set; }

    //mover para SemanticGraphNode
    public CategoryType Category { get; set; } = CategoryType.Unknown;
}

public class SemanticClassDeclarationNode : TypeDeclarationNode { }

public class SemanticInterfaceDeclarationNode : TypeDeclarationNode { }

public class SemanticRecordDeclarationNode : TypeDeclarationNode { }

public class SemanticStructDeclarationNode : TypeDeclarationNode { }

public class SemanticMethodDeclarationNode : DeclarationNode
{
    public string Name { get; set; } = string.Empty;

    public Accessibility AccessModifier { get; set; }

    public string ReturnType { get; set; } = string.Empty;

    public IEnumerable<ParameterDeclarationNode> Parameters { get; set; } = new List<ParameterDeclarationNode>();

    public string? Body { get; set; }
}