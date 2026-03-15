---
title: Capabilities
path: /docs/Capabilities.md
purpose: Summarize the repository's current implementation scenarios and developer-facing capabilities.
audience: Developers deciding how to use the repository as a reference or starting point
status: draft
last reviewed: 2026-03-15
related components:
  - /README.md
  - /docs/DocumentationManifest.md
  - /src/DataIngestionLib
  - /src/RAGDataIngestionWPF
  - /tests/RAGDataIngestionWPF.Tests.MSTest
---

# Capabilities

This document summarizes the main capabilities currently demonstrated by the repository so new developers can quickly decide where to explore next.

See also: [/docs/DocumentationManifest.md](/docs/DocumentationManifest.md)

## Current capabilities

### WPF desktop host

The repository includes a Windows desktop shell built with WPF and Generic Host integration for dependency injection, startup, navigation, and theming.

### Agent composition

The library project demonstrates how to compose AI agent behavior, session management, tool execution, and supporting services behind interfaces.

### Tool invocation

The repository includes multiple tool-style components intended to support agent interactions, such as:

- file system access within controlled boundaries
- command execution helpers
- system and environment inspection
- registry and event log access

### Chat history and context management

The solution includes SQL-backed chat history support and context injection patterns designed to keep conversation state available across turns.

### Retrieval-augmented generation support

The repository contains RAG-focused building blocks, including context injection and search strategy documentation. See `/docs/RAG Search Strategy.md` for the current retrieval notes.

### Ingestion-oriented library structure

`/src/DataIngestionLib` is organized around ingestion, retrieval, models, and services so the repository can be extended into broader documentation and data-ingestion scenarios.

### Test coverage for key behaviors

The MSTest project provides behavior-focused validation around selected services and edge cases, which helps demonstrate expected contracts for future contributors.

## Intended use as a reference repository

Developers visiting this repository can use it as an example of:

- structuring a WPF application around Generic Host and dependency injection
- separating UI orchestration from library-based agent logic
- organizing AI-adjacent tool functions and context providers inside a reusable library
- documenting a growing sample with a `/docs` manifest and repo-relative guidance

## Current limits

This documentation set is intentionally a starting point. Some areas are still evolving, and the README's "Known Issues / Work in Progress" section remains the best place to understand gaps that have already been identified.
