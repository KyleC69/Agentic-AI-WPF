---
description: "Review a proposed change for architectural fit in this repository. Use when deciding which project owns a change, which seams or interfaces to extend, and which tests, DI registrations, or docs must be updated."
name: "Architecture Review"
argument-hint: "Describe the proposed change, affected files, or feature goal"
agent: "agent"
---

Review the requested change for architectural fit in this repository.

Inputs:
- The user request or selected code
- Any mentioned files, classes, or feature area
- Repository context from [docs/DocumentationManifest.md](../../docs/DocumentationManifest.md), [docs/Architecture.md](../../docs/Architecture.md), and [docs/ContextManagement.md](../../docs/ContextManagement.md)
- Library-specific guidance from [data-ingestion-lib.instructions.md](../instructions/data-ingestion-lib.instructions.md) when the change touches DataIngestionLib

Analyze the request and return:

1. Recommended ownership
- Identify which project should own the change:
  - `src/RAGDataIngestionWPF`
  - `src/DataIngestionLib`
  - `src/RAGDataIngestionWPF.Core`
  - `tests/RAGDataIngestionWPF.Tests.MSTest`
- State why that location is correct.

2. Boundary check
- Call out any layering risks, UI leaks into library code, misplaced DI wiring, or context-management violations.
- If the request would cross boundaries incorrectly, explain the safer placement.

3. Seam and extension plan
- Identify the existing class, interface, service seam, provider, or registration point that should be extended.
- Prefer existing contracts and focused services over enlarging orchestration classes.
- Name the likely files to inspect or modify.

4. Required supporting changes
- List any required DI registration updates, settings updates, docs updates, or tests that should accompany the change.
- If a config-backed setting is involved, say whether generated settings sources also need to change.

5. Recommended execution path
- Provide a concise implementation outline in repository terms.
- If there are important alternatives, list the best one and why it is weaker.

Response rules:
- Be concrete and repo-specific.
- Prefer file and type names over generic guidance.
- Keep the answer focused on placement, seams, and follow-on changes, not full implementation.
- If the request is underspecified, make the smallest reasonable assumptions and state them.