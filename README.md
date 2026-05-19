# RoslynGraph

Ferramenta para analisar código e gerar contexto para agente dev em C# usando api do Roslyn, gera grafos de classes, métodos e invocações.

O objetivo é transformar código-fonte em uma representação estruturada contendo tipos, métodos, parâmetros e relações (como chamadas e dependências), permitindo análise, visualização e extração de contexto para LLM.

---

## Funcionalidades

- Criação de nós representativos do codigo:
  - Classes
  - Interfaces
  - Structs
  - Records
  - Métodos
- Extração de:
  - Modificadores de acesso (`public`, `private`, etc.)
  - Tipos de retorno
  - Parâmetros
  - Invocações de métodos (Construção de grafo de chamadas, call graph)
- Identificação por Id dinamico e Id estavel
- Busca por Id de nó e dependencias

---

## Melhorias futuras

Dependências externas (NuGet)
Análise incremental

## Requisitos

.NET 10

Pacotes:

- Microsoft.CodeAnalysis.CSharp
- Microsoft.CodeAnalysis.CSharp.Workspaces
- Microsoft.CodeAnalysis.Workspaces.MSBuild

## Uso básico

```csharp
using RoslynGraph.Core;
using RoslynGraph.Utils;

var graphEngine = new GraphEngine(solutionPath);

await graphEngine.FullScan();

result = await graphEngine.Search(id);

JsonHandler.PrintJson(result);
```

Há tambem a possibilidade de utilizar o RoslynGraphCLI prototipado para testes.

## 📄 Licença

Este projeto está licenciado sob a licença MIT.  
Veja o arquivo [LICENSE](./LICENSE) para mais detalhes.
