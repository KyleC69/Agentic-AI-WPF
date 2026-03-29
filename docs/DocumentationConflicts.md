---
title: Documentation vs Instructions Conflict Report
path: /docs/DocumentationConflicts.md
purpose: Highlight conflicting guidance between `/docs` content and `.github/copilot-instructions.md`.
audience: Maintainers updating documentation and contributor guidance
status: draft
last reviewed: 2026-03-28
related components:
  - /.github/copilot-instructions.md
  - /docs/DocumentationManifest.md
  - /docs/Architecture.md
  - /docs/ChangeLog.md
  - /docs/Components.md
  - /docs/RAG Search Strategy.md
---

# Documentation Conflict Report

## 1) Missing document referenced as a required entry point

- `docs/Architecture.md:100` tells developers to read `/docs/Capabilities.md`.
- `docs/ChangeLog.md:26` states `/docs/Capabilities.md` was added.
- `docs/DocumentationManifest.md:19-26` (the declared source of truth) does not list `/docs/Capabilities.md`.
- `.github/copilot-instructions.md:21` directs documentation lookup through the manifest and named docs.

**Conflict:** multiple docs reference `Capabilities.md`, but the manifest does not track it and the file is not present.

**Possible fix:** either restore `/docs/Capabilities.md` and register it in the manifest, or remove stale references from `Architecture.md` and `ChangeLog.md`.

## 2) Components doc promotes centralization in `ChatConversationService`

- `docs/Components.md:19` labels the "Main page" as `ChatConversationService` and assigns broad feature ownership to it (`docs/Components.md:21-30`).
- `.github/copilot-instructions.md:44` explicitly says to extend existing orchestration seams instead of re-centralizing logic in `ChatConversationService`.

**Conflict:** component guidance implies a central service model, while contributor instructions require distributed orchestration responsibilities.

**Possible fix:** update `docs/Components.md` to describe the split orchestration services and position `ChatConversationService` as coordinator-only.

## 3) `RAG Search Strategy` format and content conflict with repo documentation guidance

- `docs/RAG Search Strategy.md:1` contains clipboard/export metadata (`Version:1.0StartHTML...`) instead of repository document front matter.
- `docs/RAG Search Strategy.md:157-165` includes assistant-style call-to-action text ("I can help you design...").
- `.github/copilot-instructions.md:5-14` emphasizes clarity, cohesion, debuggability, and concept-focused artifacts.
- `docs/DocumentationManifest.md:30-32` defines maintenance expectations for repository documentation quality and consistency.

**Conflict:** this document reads as imported conversational output rather than maintained, repository-specific guidance expected by project instructions.

**Possible fix:** rewrite the file into repository-specific guidance (current SQL/RAG implementation, concrete entry points, and boundaries), remove conversational CTA text, and normalize structure.

## 4) Manifest maintenance note not reflected in linked docs state

- `docs/DocumentationManifest.md:32` says the manifest should be updated whenever docs are added/moved/renamed/deprecated.
- `docs/ChangeLog.md:27` claims the manifest was updated to register active docs.
- `docs/Architecture.md:100` and `docs/ChangeLog.md:26` still reference an untracked/missing `/docs/Capabilities.md`.

**Conflict:** declared maintenance process and actual linked-document state are out of sync.

**Possible fix:** perform a docs link audit as part of doc updates; ensure manifest and cross-links are updated in the same change.
