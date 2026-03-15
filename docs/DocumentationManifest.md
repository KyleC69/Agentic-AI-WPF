---
title: Documentation Manifest
path: /docs/DocumentationManifest.md
purpose: Single source of truth for developer-facing documentation in the repository.
audience: Developers discovering, evaluating, or extending the repository
status: stable
last reviewed: 2026-03-15
related components:
  - /README.md
  - /docs
  - /src/RAGDataIngestionWPF
  - /src/DataIngestionLib
  - /tests/RAGDataIngestionWPF.Tests.MSTest

---

This manifest tracks the documentation currently maintained in `/docs`.

| Title | Path | Purpose / audience | Status | Last reviewed | Related components |
| ----- | ---- | ------------------ | ------ | ------------- | ------------------ |
| Documentation Manifest | `/docs/DocumentationManifest.md` | Index of the repository's developer-facing documentation assets | stable | 2026-03-15 | `/README.md`, `/docs` |
| Change Log | `/docs/ChangeLog.md` | Project-level change log for developers who want a narrative summary of notable repository updates | draft | 2026-03-15 | `/README.md`, `/src`, `/tests` |
| Architecture | `/docs/Architecture.md` | High-level architecture tour for developers onboarding to the solution | draft | 2026-03-15 | `/src/RAGDataIngestionWPF`, `/src/DataIngestionLib`, `/src/RAGDataIngestionWPF.Core`, `/tests/RAGDataIngestionWPF.Tests.MSTest` |
| Capabilities | `/docs/Capabilities.md` | Summary of the implementation scenarios and features demonstrated by the repository | draft | 2026-03-15 | `/src/DataIngestionLib`, `/src/RAGDataIngestionWPF`, `/tests/RAGDataIngestionWPF.Tests.MSTest` |
| RAG Search Strategy | `/docs/RAG Search Strategy.md` | Notes about the repository's intended hybrid retrieval and ranking approach | draft | 2026-03-15 | `/src/DataIngestionLib`, `/docs/Architecture.md`, `/docs/Capabilities.md` |

## Maintenance Notes

- Use repo-relative paths in all documentation.
- Prefer linking back to this manifest instead of duplicating the same overview in multiple documents.
- Update this file whenever documentation is added, moved, renamed, deprecated, or substantially revised.
