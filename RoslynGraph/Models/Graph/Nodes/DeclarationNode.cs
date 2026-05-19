using Microsoft.CodeAnalysis;
using RoslynGraph.Models.Enums;
using RoslynGraph.Models.Graph.Edges;
using System.Text.Json.Serialization;

namespace RoslynGraph.Models.Graph.Nodes;
public interface IDeclarationNode
{
    string Id { get; }
    Guid IdStable { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "nodeType")]
[JsonDerivedType(typeof(ClassDeclarationNode), "class")]
[JsonDerivedType(typeof(InterfaceDeclarationNode), "interface")]
[JsonDerivedType(typeof(MethodDeclarationNode), "method")]
[JsonDerivedType(typeof(RecordDeclarationNode), "record")]
[JsonDerivedType(typeof(StructDeclarationNode), "struct")]
public abstract class DeclarationNode : IDeclarationNode
{
    public string Id { get; set; } = string.Empty;
    public Guid IdStable { get; init; } = Guid.NewGuid();

    protected DeclarationNode() { }
}

public abstract class TypeDeclarationNode : DeclarationNode
{
    public string Name { get; set; } = string.Empty;

    public Accessibility AccessModifier { get; set; }

    public CategoryType Category { get; set; } = CategoryType.Unknown;

    public List<MethodDeclarationNode> Methods { get; set; } = new();
}

public class ClassDeclarationNode : TypeDeclarationNode { }

public class InterfaceDeclarationNode : TypeDeclarationNode { }

public class RecordDeclarationNode : TypeDeclarationNode { }

public class StructDeclarationNode : TypeDeclarationNode { }

public class MethodDeclarationNode : DeclarationNode
{
    public string Name { get; set; } = string.Empty;

    public Accessibility AccessModifier { get; set; }

    public string ReturnType { get; set; } = string.Empty;

    public IEnumerable<ParameterDeclarationNode> Parameters { get; set; } = new List<ParameterDeclarationNode>();

    public string? Body { get; set; }

    public List<InvocationEdge> Invocations { get; set; } = new();
}