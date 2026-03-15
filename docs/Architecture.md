---
title: Architecture
path: /docs/Architecture.md
purpose: Explain the major solution layers, responsibilities, and runtime flow for developers new to the repository.
audience: Developers onboarding to the codebase
status: draft
last reviewed: 2026-03-15
related components:
  - /README.md
  - /docs/DocumentationManifest.md
  - /src/RAGDataIngestionWPF
  - /src/DataIngestionLib
  - /src/RAGDataIngestionWPF.Core
  - /tests/RAGDataIngestionWPF.Tests.MSTest
---

# Architecture

This document provides a developer-oriented tour of the repository structure and how the main projects fit together.

See also: [/docs/DocumentationManifest.md](/docs/DocumentationManifest.md)

## Solution overview

The repository is organized around a WPF desktop host and a reusable library that contains the AI, RAG, and data-ingestion behavior.

| Project | Path | Responsibility |
| ------- | ---- | -------------- |
| WPF application | `/src/RAGDataIngestionWPF` | Desktop shell, startup, dependency injection, navigation, and view orchestration |
| Core UI infrastructure | `/src/RAGDataIngestionWPF.Core` | Shared app services and non-agent support models used by the WPF shell |
| Data and agent library | `/src/DataIngestionLib` | Agent composition, tool functions, RAG context injection, chat history, and ingestion logic |
| Tests | `/tests/RAGDataIngestionWPF.Tests.MSTest` | MSTest coverage for repository behavior and edge cases |

## Layering

### 1. UI layer

`/src/RAGDataIngestionWPF` contains the WPF application. It is responsible for:

- building the Generic Host in `App.xaml.cs`
- registering pages, view models, and services with DI
- starting the shell and navigation flow
- presenting agent output and settings to the user

The UI should orchestrate work rather than own business logic.

### 2. Core library layer

`/src/DataIngestionLib` contains the repository's core implementation details, including:

- agent creation and session management
- AI and RAG context providers
- tool implementations exposed to the agent
- SQL-backed chat history and data access
- document and API ingestion workflows

This project is the main place to look when you want to understand the repository's AI behavior.

### 3. Supporting infrastructure layer

`/src/RAGDataIngestionWPF.Core` contains shared application infrastructure that supports the UI host without taking a dependency on the WPF views themselves.

### 4. Test layer

`/tests/RAGDataIngestionWPF.Tests.MSTest` validates behavior with MSTest. The tests are the best executable reference for expected behavior around context injection, identity, and tool boundaries.

## Runtime flow

At a high level, the application starts and runs through the following sequence:

1. `App.xaml.cs` creates and configures the Generic Host.
2. Services, view models, pages, and hosted services are registered with DI.
3. `ApplicationHostService` activates the shell window.
4. The UI resolves services from DI and forwards user actions into the library layer.
5. `DataIngestionLib` composes the agent pipeline, builds context, executes tools, and persists conversation state.

## Key architectural themes

### Dependency injection

The repository uses the Generic Host pattern so services can be composed consistently across the UI and library projects.

### Separation of concerns

The WPF project is the composition root and presentation layer. The reusable logic lives in library projects so it can evolve independently of the UI.

### Persistent context

The repository uses SQL-backed chat history and RAG context providers to preserve state and rehydrate relevant context across turns.

### Local-first developer workflow

The repository is designed for local development with a Windows desktop shell, .NET 10 preview tooling, and repository-relative source organization.

## Suggested entry points for new developers

- Start with `/README.md` for setup and repository positioning.
- Read `/docs/Capabilities.md` for a feature-oriented overview.
- Explore `/src/RAGDataIngestionWPF/App.xaml.cs` to understand startup and DI wiring.
- Explore `/src/DataIngestionLib` for the core agent and ingestion implementation.
