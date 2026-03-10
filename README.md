# RAGDataIngestionWPF

A **WPF desktop application** that demonstrates key capabilities of the [Microsoft Agent Framework (MAF)](https://learn.microsoft.com/agent-framework/overview/agent-framework-overview) — including AI agent construction, tool invocation, SQL-backed chat history, RAG (Retrieval-Augmented Generation) context injection, and middleware pipeline composition — all hosted within a Generic Host + DI WPF shell.

> **Status:** Active development / reference implementation. Targets **.NET 10 Preview** and **Microsoft Agent Framework 1.0-rc3**.

---

## Table of Contents

- [Project Purpose](#project-purpose)
- [Architecture Overview](#architecture-overview)
- [Solution Structure](#solution-structure)
- [Key Agent Framework Capabilities Demonstrated](#key-agent-framework-capabilities-demonstrated)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Running Tests](#running-tests)
- [Known Issues / Work in Progress](#known-issues--work-in-progress)

---

## Project Purpose

`RAGDataIngestionWPF` is an **example application** whose primary goal is to show how to compose a production-quality AI agent desktop application using:

- **Microsoft Agent Framework** (`Microsoft.Agents.AI` + `Microsoft.Agents.AI.Workflows`) for agent creation, session management, context providers, and middleware pipelines.
- **Microsoft.Extensions.AI** abstractions for provider-agnostic chat client integration.
- **Ollama** as the local LLM provider (swappable via DI).
- **SQL Server / LocalDB** for durable, conversation-scoped chat history with EF Core 10.
- **RAG (Retrieval-Augmented Generation)** via a hybrid SQL Server search stack (vector, semantic, full-text).
- **WPF + MahApps.Metro** for the desktop UI shell with Generic Host lifecycle management.

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                      RAGDataIngestionWPF (WPF UI)                │
│  App.xaml.cs (Generic Host / DI root)                            │
│  ViewModels  ←→  IChatConversationService                        │
└────────────────────────┬─────────────────────────────────────────┘
                         │ depends on
┌────────────────────────▼─────────────────────────────────────────┐
│                        DataIngestionLib                           │
│                                                                   │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │  Agents                                                      │ │
│  │  ├── AgentFactory         — builds AIAgent via MAF pipeline  │ │
│  │  └── ChatConversationService — orchestrates agent sessions   │ │
│  ├─────────────────────────────────────────────────────────────┤ │
│  │  ToolFunctions (AIFunctionFactory tools exposed to agent)   │ │
│  │  ├── SandboxFileReader / SandboxFileWriter                   │ │
│  │  ├── WebSearchPlugin      — external search via LangSearch   │ │
│  │  ├── SystemInfoTool       — OS / machine info                │ │
│  │  ├── AgentLogger          — structured agent activity log    │ │
│  │  └── SafeCommandRunner    — sandboxed shell command runner   │ │
│  ├─────────────────────────────────────────────────────────────┤ │
│  │  Services / Context Injectors (MAF MessageAIContextProvider) │ │
│  │  ├── AIContextHistoryInjector  — SQL-backed history window   │ │
│  │  ├── AIContextHistoryInjector2a — MAF pipeline variant      │ │
│  │  └── AIContextRAGInjector      — RAG source aggregator      │ │
│  ├─────────────────────────────────────────────────────────────┤ │
│  │  EFModels / SqlChatHistoryProvider                           │ │
│  │  └── KnowledgeBaseContext  — EF Core + stored procedures     │ │
│  └─────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────┘
                         │
┌────────────────────────▼─────────────────────────────────────────┐
│                    RAGDataIngestionWPF.Core                       │
│  File, Identity, Graph, SampleData services (UI infrastructure)  │
└──────────────────────────────────────────────────────────────────┘
```

### Key Architectural Layers

| Layer | Responsibility |
|---|---|
| **RAGDataIngestionWPF** (UI) | WPF shell, DI root, view-models, navigation, theming |
| **DataIngestionLib** | All AI, agent, RAG, and history logic — no UI dependencies |
| **RAGDataIngestionWPF.Core** | UI infrastructure: file service, identity stub, MS Graph |
| **Tests.MSTest** | Unit tests for `DataIngestionLib` services and injectors |

---

## Solution Structure

```
RAGDataIngestionWPF/
├── RAGDataIngestionWPF/            # WPF application (UI shell)
│   ├── App.xaml.cs                 # Generic Host + DI root
│   ├── ViewModels/                 # MVVM view-models
│   ├── Views/                      # XAML pages
│   ├── Services/                   # Navigation, theming, identity cache
│   └── appsettings.json            # Configuration (AI endpoint, history DB)
│
├── DataIngestionLib/               # Core AI and agent library
│   ├── Agents/
│   │   ├── AgentFactory.cs         # Builds and configures AIAgent
│   │   └── ChatConversationService.cs  # Manages agent session lifecycle
│   ├── Contracts/                  # Interfaces (ISP-aligned)
│   │   └── Services/
│   │       ├── IAgentFactory.cs
│   │       ├── IChatConversationService.cs
│   │       ├── IChatHistoryMemoryProvider.cs  # Narrow: read/write
│   │       ├── IAIContextHistoryInjector.cs   # Full: prune/update/delete
│   │       ├── IAgentIdentityProvider.cs
│   │       ├── IRagContextSource.cs
│   │       └── IRuntimeContextAccessor.cs
│   ├── Services/
│   │   ├── ContextInjectors/
│   │   │   ├── AIContextHistoryInjector.cs    # SQL-backed MAF context provider
│   │   │   ├── AIContextHistoryInjector2a.cs  # MAF MessageAIContextProvider variant
│   │   │   └── AIContextRAGInjector.cs        # RAG source aggregator
│   │   ├── FixedAgentIdentityProvider.cs
│   │   ├── SqlChatHistoryProvider.cs
│   │   └── ChatHistoryInitializationService.cs
│   ├── ToolFunctions/
│   │   ├── ToolBuilder.cs          # Assembles tool list for agent
│   │   ├── SandboxFileReader.cs    # Sandboxed file read tool
│   │   ├── SandboxFileWriter.cs    # Sandboxed file write tool
│   │   ├── WebSearchPlugin.cs      # Web search via LangSearch API
│   │   ├── SystemInfoTool.cs       # OS/machine information tool
│   │   ├── AgentLogger.cs          # Agent activity logger tool
│   │   ├── SafeCommandRunner.cs    # Sandboxed command runner
│   │   ├── EventLogReader.cs       # Windows Event Log reader
│   │   ├── RagSearchTool.cs        # RAG retrieval tool (placeholder)
│   │   ├── RegistryReaderTool.cs   # Windows Registry reader
│   │   └── AgentRunMiddleWare.cs   # Agent pipeline middleware (WIP)
│   ├── EFModels/                   # EF Core models + KnowledgeBaseContext
│   ├── Models/                     # AIChatMessage, ChatHistory, etc.
│   └── Options/                    # ChatHistoryOptions, ChatSessionOptions
│
├── RAGDataIngestionWPF.Core/       # UI infrastructure library
│   ├── Services/                   # FileService, IdentityService, GraphService
│   └── Models/                     # User, SampleOrder, SampleCompany
│
└── RAGDataIngestionWPF.Tests.MSTest/  # Unit tests
    ├── AIContextHistoryInjectorTests.cs
    ├── EventLogReaderTests.cs
    └── FixedAgentIdentityProviderTests.cs
```

---

## Key Agent Framework Capabilities Demonstrated

### 1. AI Agent Construction with ChatClientBuilder Pipeline

`AgentFactory` demonstrates how to assemble a `Microsoft.Agents.AI.AIAgent` from an `IChatClient` using the fluent `ChatClientBuilder` pipeline:

```csharp
IChatClient outer = new ChatClientBuilder(_innerClient)
    .UseLogging(_factory)
    .UseFunctionInvocation()
    .ConfigureOptions(chatOptions =>
    {
        chatOptions.Instructions = _modelInstructions;
        chatOptions.Temperature = 0.7f;
    })
    .Build();

return outer.AsAIAgent(tools: ToolBuilder.GetAiTools());
```

This wires together:
- **Logging middleware** — structured logging of every request/response
- **Function invocation middleware** — automatic tool dispatch from model responses
- **System instructions** — injected as the agent's persona/constraints
- **Tool registration** — `AIFunctionFactory`-created tools exposed to the model

### 2. Agent Sessions (`AgentSession`)

`ChatConversationService` shows how to create and reuse an `AgentSession` for multi-turn conversations:

```csharp
_agentSession = _agent.CreateSessionAsync().Result;
// Later, per turn:
AgentResponse response = await _agent.RunAsync(userMessage, _agentSession, null, cancellationToken);
```

Sessions maintain conversation context across turns without manual message threading.

### 3. Tool Functions via `AIFunctionFactory`

Agent tools are registered using `Microsoft.Extensions.AI.AIFunctionFactory.Create`, making .NET methods callable by the LLM:

| Tool | Class | Capability |
|---|---|---|
| File read | `SandboxFileReader` | Read files within a sandboxed directory |
| File write | `SandboxFileWriter` | Write files within a sandboxed directory |
| Web search | `WebSearchPlugin` | Query LangSearch API and return results |
| System info | `SystemInfoTool` | Return OS, machine name, .NET version |
| Agent logger | `AgentLogger` | Append timestamped log entries |
| Shell commands | `SafeCommandRunner` | Execute an allow-listed set of shell commands |

### 4. SQL-Backed Durable Chat History

`SqlChatHistoryProvider` + `AIContextHistoryInjector` provide a full conversation persistence stack:

- Messages are stored per `(conversationId, sessionId, agentId, userId, applicationId)`.
- `AIContextHistoryInjector.BuildContextMessagesAsync` retrieves a windowed view of history (capped by `MaxContextMessages`) and deduplicates against the current request.
- `PruneConversationAsync` trims oldest messages when the conversation exceeds the configured limit.
- `ChatHistoryInitializationService` (an `IHostedService`) ensures the database is initialized and restores the last active session on startup.

### 5. RAG Context Injection (`AIContextRAGInjector`)

`AIContextRAGInjector` extends `MessageAIContextProvider` to aggregate context from multiple `IRagContextSource` implementations and prepend them to each agent invocation. This enables a hybrid retrieval strategy:

- Vector search
- Semantic search
- Full-text search

All backed by SQL Server with EF Core (`KnowledgeBaseContext`).

### 6. Interface Segregation for Context Providers

The contract hierarchy demonstrates ISP in practice:

- `IChatHistoryMemoryProvider` — narrow interface: `BuildContextMessagesAsync` + `StoreMessagesAsync`
- `IAIContextHistoryInjector` — extends with full lifecycle: `PruneConversationAsync`, `UpdateMessageContentAsync`, `DeleteMessageAsync`
- `IAgentIdentityProvider` — decouples agent ID from hard-coded strings; `FixedAgentIdentityProvider` provides the startup default

### 7. Generic Host Integration

`App.xaml.cs` registers all services via `IHostBuilder` / `IServiceCollection`, following the WPF Generic Host pattern:

- `ApplicationHostService` controls window activation as an `IHostedService`
- `ChatHistoryInitializationService` initializes the history DB on startup
- `ToastNotificationActivationHandler` handles toast-activated launch

### 8. Declarative Workflow Support

The `DataIngestionLib.csproj` references:
- `Microsoft.Agents.AI.Workflows` — for graph-based multi-step agent workflows
- `Microsoft.Agents.AI.Declarative` — for declarative agent configuration
- `Microsoft.Agents.AI.A2A` — for agent-to-agent communication patterns

These packages enable composing agents into sequential, concurrent, or hand-off orchestration patterns.

---

## Technology Stack

| Component | Package / Version |
|---|---|
| AI Agent Framework | `Microsoft.Agents.AI` 1.0.0-rc3 |
| Agent Workflows | `Microsoft.Agents.AI.Workflows` 1.0.0-rc3 |
| Agent A2A | `Microsoft.Agents.AI.A2A` 1.0.0-preview |
| AI Abstractions | `Microsoft.Extensions.AI` 10.3.0 |
| LLM Provider | [OllamaSharp](https://github.com/awaescher/OllamaSharp) 5.4.23 (local Ollama) |
| ORM | EF Core 10 (`Microsoft.EntityFrameworkCore.SqlServer`) |
| Database | SQL Server / LocalDB |
| UI Framework | WPF on .NET 10 Preview |
| UI Theming | MahApps.Metro 3.0.0-rc |
| MVVM Toolkit | CommunityToolkit.Mvvm 8.4 |
| Notifications | Microsoft.Toolkit.Uwp.Notifications 7.1.3 |
| DI / Hosting | Microsoft.Extensions.Hosting 11 Preview |
| Testing | MSTest + Moq |

---

## Prerequisites

- **Windows 10 / 11** (WPF + Windows Event Log / Registry tools require Windows)
- **.NET 10 Preview SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Visual Studio 2022 17.13+** or **VS 2026 Preview** (for .NET 10 WPF tooling)
- **Ollama** — [Install](https://ollama.ai/) and pull the desired model:
  ```bash
  ollama pull <model-name>
  ```
- **SQL Server** or **LocalDB** for chat history persistence
- **LangSearch API key** (optional) — set `LANGAPI_KEY` environment variable to enable web search tool

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/KyleC69/RAGDataIngestionWPF.git
cd RAGDataIngestionWPF
```

### 2. Configure the application

The default `appsettings.json` uses `Server=.` (local default SQL Server instance). To override with your own server name, create a **git-ignored** local override file:

```powershell
# PowerShell (Windows)
Copy-Item RAGDataIngestionWPF\appsettings.Development.json.example RAGDataIngestionWPF\appsettings.Development.json
```

Then edit `RAGDataIngestionWPF/appsettings.Development.json` with your SQL Server instance:

```jsonc
{
  "ChatHistory": {
    "ConnectionString": "Server=YOUR_SERVER_NAME;Database=AIChatHistory;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;"
  }
}
```

> **Note:** `appsettings.Development.json` is excluded from source control (listed in `.gitignore`). Always use this file for machine-specific overrides — never edit `appsettings.json` directly with a local server name.

Alternatively, override the connection string via an environment variable (no file required):

```powershell
# PowerShell
$env:ChatHistory__ConnectionString = "Server=YOUR_SERVER_NAME;Database=AIChatHistory;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;"
```

### 3. Set the Ollama model

In `RAGDataIngestionWPF/App.xaml.cs`, update the model constant to match a model you have pulled:

```csharp
private const string OllamaModel = "llama3.2"; // or whichever model you have
```

### 4. Build and run

```bash
dotnet build RAGDataIngestionWPF.sln
dotnet run --project RAGDataIngestionWPF/RAGDataIngestionWPF.csproj
```

Or open `RAGDataIngestionWPF.sln` in Visual Studio and press **F5**.

---

## Configuration

| Section | Key | Default | Description |
|---|---|---|---|
| `AppConfig` | `configurationsFolder` | `RAGDataIngestionWPF\Configurations` | Local config storage folder |
| `AppConfig` | `chatSessionFileName` | `ChatSession.json` | Chat session persistence file |
| `AppConfig` | `userFileName` | `User.json` | User profile file |
| `ChatHistory` | `ConnectionString` | `Server=.` (local default) | SQL Server connection for chat history |
| `ChatHistory` | `MaxContextMessages` | `40` | Max messages retained in context window |
| `ChatHistory` | `MaxContextTokens` | `120000` | Token budget for context |
| `ChatHistory` | `EnableSummarization` | `false` | Enable automatic history summarization |

### Environment Variables

| Variable | Required | Description |
|---|---|---|
| `LANGAPI_KEY` | Optional | API key for LangSearch web search tool |

---

## Running Tests

```bash
dotnet test RAGDataIngestionWPF.Tests.MSTest
```

The test project covers:
- `AIContextHistoryInjector` — context windowing, deduplication, pruning, persistence
- `EventLogReader` — constructor validation, result factory methods
- `FixedAgentIdentityProvider` — guard clause and identity contract

---

## Known Issues / Work in Progress

The following items are tracked as open issues in this repository:

- **`EventLogReader.cs`** — `SandboxEventLogReader`, `EventLogReadResult`, and `EventLogEntryDto` classes are declared in the global namespace instead of `DataIngestionLib.ToolFunctions`.
- **`RegistryReaderTool.cs`** — Contains unreachable code, an undefined `value` variable reference, and uses a static `LoggerFactory.Create()` instead of constructor-injected `ILogger`.
- **`AgentRunMiddleWare.cs`** — Dead code: empty `run2()` method, `Console.WriteLine` used instead of the injected `ILoggerFactory`, and the class is not integrated into the agent pipeline.
- **`ChatConversationService.cs`** — Blocking `.Result` call on `CreateSessionAsync()` in the constructor creates a deadlock risk. `FormatMarkdownLite` private method is declared but never called.
- **`WebSearchPlugin.cs`** — ~~Creates `HttpClient` directly with `new()` instead of using `IHttpClientFactory`, bypassing connection pooling and lifetime management.~~ Fixed: now accepts `IHttpClientFactory` via constructor injection and uses the named `"langsearch"` client.
- **`RagSearchTool.cs`** — Generic type parameter incorrectly named `IRagRetriever` (shadows the interface of the same name), and `Search()` returns an empty stub.
- **`FileSystemPlugin.cs`** — ~~Class named `FileSystemSearch` but its `WriteText` method writes files rather than searching; description is misleading.~~ Fixed: class renamed to `FileSystemPlugin` and `WriteText` description corrected.
- **`appsettings.json`** — Connection string contains a hard-coded machine name (`Server=Desktop-nc01091`). Should use `(localdb)\MSSQLLocalDB` or a placeholder.

See the [Issues tab](../../issues) for full details and status.
