---
description: "Use when editing DataIngestionLib services, contracts, providers, agent composition, RAG context injection, chat history, ingestion workflows, or tool functions. Keeps DataIngestionLib UI-agnostic, interface-first, and aligned with the repo's context architecture."
name: "DataIngestionLib Guidelines"
applyTo: "src/DataIngestionLib/**"
---

# DataIngestionLib Guidelines

- Keep DataIngestionLib UI-agnostic. Do not introduce WPF, view-model, navigation, or shell dependencies here; composition and DI wiring belong in src/RAGDataIngestionWPF/App.xaml.cs.
- Extend existing seams before enlarging orchestration classes. Prefer adding or refining an interface in src/DataIngestionLib/Contracts plus a focused implementation in Services or Providers over adding more direct responsibilities to ChatConversationService, AgentFactory, or other coordinators.
- Preserve the context architecture documented in docs/ContextManagement.md: permanent SQL history, semi-volatile conversation cache, durable progress logs, and RAG sources stay distinct. Do not collapse them into a single store, rely on summarization-only memory, or replay raw tool output as context.
- Keep identity and configuration access centralized through existing abstractions. Reuse IAppSettings and the current identity providers instead of adding ad hoc environment or UI-setting reads inside conversation, RAG, history, or tool flows.
- Prefer constructor injection, ArgumentNullException.ThrowIfNull(...), async APIs with CancellationToken, and ConfigureAwait(false) in library async code.
- When a library change requires a new app-backed setting exposed through IAppSettings, also update the generated settings source in src/RAGDataIngestionWPF/Properties so runtime and UI settings stay aligned.
- For new chat, history, RAG, or tool behavior, add or update deterministic MSTest coverage under tests/RAGDataIngestionWPF.Tests.MSTest and prefer testing the focused seam you introduced rather than only top-level orchestration.
- Start documentation lookup at docs/DocumentationManifest.md, then use docs/Architecture.md and docs/ContextManagement.md instead of duplicating large design explanations in code comments or new markdown files.