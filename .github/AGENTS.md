---
title: AGENTS
path: .github/AGENTS.md
purpose: Provide concise guidance for AI coding agents to be productive in this repository.
---

# Overview
This file gives AI agents the essential information they need to work effectively in the **Agentic-AI-WPF** solution. It references the full documentation rather than duplicating it.

## Build & Test Commands
- **Build WPF app**: `dotnet build src/RAGDataIngestionWPF/RAGDataIngestionWPF.csproj`
- **Build core library**: `dotnet build src/DataIngestionLib/DataIngestionLib.csproj`
- **Run tests**: `dotnet test tests/AgenticAIWPF.Tests.MSTest/AgenticAIWPF.Tests.MSTest.csproj`

## Architecture Snapshot
- **UI layer** – `src/RAGDataIngestionWPF` (composition root, DI, navigation)
- **Core UI infrastructure** – `src/RAGDataIngestionWPF.Core`
- **Agent & RAG library** – `src/DataIngestionLib` (agent pipeline, tool functions, SQL chat history)
- **Tests** – `tests/AgenticAIWPF.Tests.MSTest`

For a detailed description see the [Architecture document](/docs/Architecture.md).

## Project Conventions
- Follow the **AI MANDATORY Constraints** in [.github/copilot-instructions.md](.github/copilot-instructions.md).
- Use 4‑space indentation, CRLF line endings, and keep blank‑line‑heavy style.
- Prefer constructor injection and `Guard.ThrowIfNull(...)` for argument validation.
- Keep constants in `UPPER_SNAKE_CASE`.
- Internal methods should be marked `internal` for testability unless they need to be public.
- UI code must stay UI‑agnostic; business logic belongs in `DataIngestionLib`.

## Common Pitfalls
- Transient generated‑file errors (`obj/*.g.cs` missing) – clean `src/RAGDataIngestionWPF/obj` and `bin` then rebuild.
- Accessing files outside the project boundaries is prohibited by the mandatory constraints.
- Ensure all project files are included in the `.csproj` or `.sln`; agents must not reference files outside these boundaries.

## Helpful Links
- [README.md](/README.md) – setup and repository overview
- [Documentation Manifest](/docs/DocumentationManifest.md) – index of all docs
- [Context Management](/docs/ContextManagement.md) – how chat history and RAG context are stored
- [Components](/docs/Components.md) – description of major components

---

*This file is intentionally concise; agents should follow the “link, don’t embed” principle and refer to the full documentation for details.*
