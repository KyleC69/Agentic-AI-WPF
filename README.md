---

name: README.md
description: README for RAGDataIngestionWPF repository
updated: 2026-03-24

---

Last Update: 3/24/26

# RAGDataIngestionWPF

A WPF desktop application and supporting libraries demonstrates creating Agentic AI's with Microsoft's Agent Framework using local models, including tool function calling. Due to new framework advancements, it now supports RAG-oriented context handling through middleware components that can manage retrieval, history, and context injection in a flexible way. The current implementation includes a SQL Server-based chat history provider and context injector that can leverage new SQL Server 2025 features for vector search and BM25-based ranking to enhance the agent's retrieval capabilities. The project is designed to be extensible and adaptable, allowing for different configurations of context management and retrieval strategies as needed.

**Hard limit to .Net10 by design to target Windows 10 at minimum.**

*Status: active development. The current solution targets .NET 10 Windows TFMs and `DataIngestionLib` currently references Microsoft Agent Framework `1.0.0-rc4` packages.*

> **Important Constraints:** This projects intent is to create an Agentic AI using local models as if in an air-gapped environment. As of today there are no libraries that fully implement the Agent Framework with the ability to use a local model by file with tools and tool function calling. I tried many when I started this project including ONNXRuntime and connectors in Semantic Kernel. The only library that I found that could work was OllamaSharp, paired with the Ollama local model server and Caddy to act as reverse proxy for SQL operations. This requires the Ollama server to run locally and for the application to set the endpoint to the local Ollama server. This is the only way I found to use a local model with the Agent Framework and tool function calls it would'nt be Agentic without tools, which is a core requirement for this project.

**Additionally** , this project uses preview features within SQL server 2025 for vector search capabilities and internal functions in preview which include using external LLM's within SQL in stored procedures. This means that the project requires a SQL Server 2025 instance with the appropriate preview features enabled to fully utilize the chat history and retrieval components. SQL Server 2025 is currently in preview and available for download from the Microsoft website. I highly recommend using SSMS for SQL Server management and query editing. Some advanced features and the Vector datatype are not fully recognized in VS2026s SQL Server Object Explorer.

---

## Table of Contents

- [Project Purpose](#project-purpose)
- [Quick Start - Without SQL](#quick-start)
- [Documentation](#documentation)
- [Solution Structure](#solution-structure)
- [Current Implementation Highlights](#current-implementation-highlights)
- [Technology Stack](#technology-stack)
- [SQL Server 2025 Dependency](#sql-server-2025-dependency)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Running Tests](#running-tests)

## Project Purpose

`RAGDataIngestionWPF` currently contains:

- a WPF composition root in `src/RAGDataIngestionWPF`
- a UI-agnostic agent, ingestion, history, and tool library in `src/DataIngestionLib`
- shared UI infrastructure in `src/RAGDataIngestionWPF.Core`
- an MSTest suite in `tests/RAGDataIngestionWPF.Tests.MSTest`

The repository is useful as a reference for wiring together a desktop shell, host-based dependency injection, Microsoft Agent Framework integrations, and a growing set of local Windows diagnostics tools.

## Quick Start

I have gated SQL operation behind a compiler directive to allow the project to build and run without SQL Server, but for the full experience including chat history and RAG context management, SQL Server 2025 is required.
To run the project without SQL Server, simply ensure that the `SQL` compiler directive is not defined in the project settings. This will allow the application to use in-memory implementations for chat history and context management, enabling you to explore the agent's capabilities without setting up a SQL Server instance.

## Documentation

The `docs` folder currently contains these developer-facing entry points:

- [`/docs/DocumentationManifest.md`](/docs/DocumentationManifest.md) - index of maintained documentation
- [`/docs/Architecture.md`](/docs/Architecture.md) - high-level solution and layering overview
- [`/docs/Components.md`](/docs/Components.md) - component inventory across the solution
- [`/docs/ContextManagement.md`](/docs/ContextManagement.md) - context, history, and RAG state model
- [`/docs/ChangeLog.md`](/docs/ChangeLog.md) - narrative change log for notable repository updates
- [`/docs/RAG Search Strategy.md`](/docs/RAG%20Search%20Strategy.md) - repository notes about retrieval strategy

The `sql` folder contains SQL scripts used to set up the database components of the solution, including stored procedures, triggers, and table definitions.

- [`/sql/README.md`](/sql/README.md) - important notes on SQL database dependencies, setup, and configuration for the project

Start with the manifest if you want the quickest route to the right document.

## Solution Structure

```text
RAGDataIngestionWPF/
├── docs/                               # Developer-facing documentation
├── src/
│   ├── RAGDataIngestionWPF/            # WPF application and composition root
│   ├── DataIngestionLib/               # Agent, RAG, ingestion, and tool library
│   └── RAGDataIngestionWPF.Core/       # Shared UI infrastructure
├── tests/
│   └── RAGDataIngestionWPF.Tests.MSTest/  # MSTest unit and integration coverage
└── SolutionFix/                        # A no-op project to fix Solution Explorer - Maintains proper visual of solution structure without affecting build or dependencies
```

### Current Projects

| Project | Current role |
| --- | --- |
| `src/RAGDataIngestionWPF` | WPF app, host startup, views, view models, navigation, theming, and application orchestration |
| `src/DataIngestionLib` | AI agent composition, contracts, services, providers, ingestion workflows, models, and agent-visible tools |
| `src/RAGDataIngestionWPF.Core` | Shared UI-supporting contracts, helpers, models, and services |
| `tests/RAGDataIngestionWPF.Tests.MSTest` | MSTest coverage for library, UI-supporting services, and integration slices |

## Current Implementation Highlights

The repository currently includes the following observable implementation areas:

- WPF host composition in `src/RAGDataIngestionWPF/App.xaml.cs`
- agent orchestration and chat services under `src/DataIngestionLib/Agents` and `src/DataIngestionLib/Services`
- contracts and service seams under `src/DataIngestionLib/Contracts`
- ingestion and provider code under `src/DataIngestionLib/DocIngestion`, `src/DataIngestionLib/Providers`, and related folders
- staged retrieval/search strategy in `src/DataIngestionLib/Providers/SqlChatHistoryProvider.cs`: broad full-text retrieval first, BM25-based concentration/ranking next, and semantic vector-similarity refinement when vector matches are available
- read-only agent tool registration through `src/DataIngestionLib/ToolFunctions/ToolBuilder.cs`
- a Windows diagnostics tool set that currently includes file read, web search, system info, event log access, event channel access, registry reads, WMI reads, service health, startup inventory, storage health, network configuration, process snapshots, performance counters, reliability history, installed updates, and a bounded command runner
- MSTest coverage for unit, boundary, host, UI-supporting, and integration scenarios in `tests/RAGDataIngestionWPF.Tests.MSTest`

## Technology Stack

| Component | Package / Version |
| --- | --- |
| AI Agent Framework | `Microsoft.Agents.AI` 1.0.0-rc4 |
| Agent Builder | `Microsoft.Agents.Builder` 1.5.60-beta |
| AI Abstractions | `Microsoft.Extensions.AI` 10.4.1 |
| LLM Provider | [OllamaSharp](https://github.com/awaescher/OllamaSharp) 5.4.24 |
| ORM | EF Core 10.0.3 (`Microsoft.EntityFrameworkCore.SqlServer`) |
| Database integrations | SQL Server-oriented history and retrieval components in `DataIngestionLib` |
| UI Framework | WPF on .NET 10 Preview |
| UI Theming | MahApps.Metro 3.0.0-rc0529 |
| Hosting / logging | Microsoft.Extensions.Hosting 10.0.5 |
| MVVM Toolkit | CommunityToolkit.Mvvm 8.4.0 |
| Notifications | Microsoft.Toolkit.Uwp.Notifications 7.1.3 |
| Testing | MSTest 4.1.0 + Moq 4.20.72 |

## SQL Server 2025 Dependency

The repository's chat history and RAG context management components currently depend on SQL Server 2025 for enhanced retrieval capabilities. This includes the use of vector search features, BM25-based ranking, and full-text search to provide more relevant context to the agent during conversations. The SQL-based implementation allows for persistent chat history storage, advanced retrieval strategies, and the ability to leverage SQL Server's performance optimizations for handling large volumes of conversational data.

SQL Server dependency can be easily removed by removing the SQL-based provider and context injector from the agent configuration, which will cause the agent to fall back to in-memory history and context management. However, this will limit the agent's ability to retrieve relevant historical context across sessions and reduce the overall effectiveness of the RAG strategy. For rapid out of the box operation, the in-memory configuration allows the agent to operate without any external dependencies, but for a more robust and capable implementation, SQL Server 2025 is recommended.

For Data Ingestion SQL Server is mandatory as the ingestion workflows are built around SQL-based storage and retrieval of documents and related metadata. The ingestion processes rely on SQL Server's capabilities to manage and query the ingested data effectively, making it a critical component of the overall solution.

## Prerequisites

- Windows 10 or 11
- .NET 10 Preview SDK
- Visual Studio 2022 or Visual Studio Preview with .NET desktop tooling if you want to run the WPF app interactively
- Ollama if you want to exercise the local chat model settings in the app
- SQL Server if you want to exercise chat history or related database-backed features
- local Windows access for the Windows diagnostics tools and integration tests

Optional external dependency:

- `LANGAPI_KEY` to enable the web-search tool surface

Example Ollama setup:

```bash
ollama pull <model-name>
```

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/KyleC69/RAGDataIngestionWPF.git
cd RAGDataIngestionWPF
```

### 2. Build the main projects

```bash
dotnet build src/DataIngestionLib/DataIngestionLib.csproj
dotnet build src/RAGDataIngestionWPF/RAGDataIngestionWPF.csproj
```

### 3. Open the solution

Open `RAGDataIngestionWPF.slnx` in Visual Studio if you want to run or debug the WPF application.

### 4. Configure local settings as needed

The app currently ships with `src/RAGDataIngestionWPF/App.config` and generated settings files under `src/RAGDataIngestionWPF/Properties`.
Machine-specific values such as model selection, host information, and database connection strings should be supplied through local settings rather than committed repository edits.

Key settings visible in `App.config` include:

- `OllamaHost`
- `OllamaPort`
- `ChatModel`
- `EmbeddingModel`
- `ChatHistoryConnectionString`
- `RemoteRAGConnectionString`

## Configuration

Configuration is currently split across:

- `src/RAGDataIngestionWPF/App.config`
- `src/RAGDataIngestionWPF/Properties/Settings.settings`
- machine-local user settings generated by the .NET settings infrastructure

The repository does not currently expose an `appsettings.json`-based configuration story at the root README level. When updating settings-backed behavior, inspect the WPF project's settings files and the code paths that consume them.

### Environment Variables

| Variable | Required | Description |
| --- | --- | --- |
| `LANGAPI_KEY` | Optional | API key used by the web-search tool |

## Running Tests

Run the full MSTest project with:

```bash
dotnet test tests/RAGDataIngestionWPF.Tests.MSTest/RAGDataIngestionWPF.Tests.MSTest.csproj
```

The test project currently includes:

- broad unit coverage across conversation services, providers, models, view-model support code, and tool functions
- focused boundary tests for the Windows diagnostics tools
- integration-tagged diagnostics suites

Run only the integration-tagged tests with:

```bash

dotnet test tests/RAGDataIngestionWPF.Tests.MSTest/RAGDataIngestionWPF.Tests.MSTest.csproj --filter "TestCategory=Integration"
