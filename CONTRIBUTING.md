# Contributing to AgenticAIWPF

Thank you for taking the time to contribute.

## Start Here

Before making changes, review these repository entry points:

- `/README.md`
- `/docs/DocumentationManifest.md`
- `/docs/Architecture.md`
- `/docs/Components.md`
- `/docs/ContextManagement.md`
- `/CODE_OF_CONDUCT.md`

If you plan to make a significant change, open an issue or start a discussion first so the work can be aligned before implementation begins.

## Development Environment

This repository is intended for Windows development.

Recommended setup:

- Windows 10 or 11
- .NET 10 Preview SDK
- Visual Studio 2022 or Visual Studio Preview with WPF tooling
- Ollama for local model-backed features
- SQL Server 2025 if you need database-backed chat history or RAG features

## Build and Test

Use the existing project commands from the repository root:

```powershell
dotnet build src/AgentAILib/AgentAILib.csproj
dotnet build src/AgenticAIWPF/AgenticAIWPF.csproj
dotnet test tests/AgenticAIWPF.Tests.MSTest/AgenticAIWPF.Tests.MSTest.csproj
```

If the WPF project fails with transient generated-file errors from `obj` output, clean `src/AgenticAIWPF/bin` and `src/AgenticAIWPF/obj`, then rebuild.

## Contribution Guidelines

Keep changes focused, reviewable, and easy to trace.

- Keep the WPF composition root in `/src/AgenticAIWPF`.
- Keep UI-agnostic agent, ingestion, chat, RAG, and tool logic in `/src/AgentAILib`.
- Keep shared UI-supporting infrastructure in `/src/AgenticAIWPF.Core`.
- Avoid introducing new layers, wrappers, or abstractions unless they clearly improve clarity or reuse.
- Prefer constructor injection and guard clauses such as `Guard.ThrowIfNull(...)`.
- Prefer async APIs with cancellation support when changing existing async flows.
- Use `IUserIdentityProvider` for runtime user identity in conversation paths instead of new direct `Environment.UserName` reads.
- Surface user-configurable settings through the Settings UI instead of hardcoding machine-specific values.
- Do not commit secrets, credentials, or machine-local configuration values.

## Code Style

Follow the repository's existing conventions.

- Respect `/.editorconfig`.
- Preserve CRLF line endings.
- Use 4 spaces for C#, XAML, and related source files.
- Use tabs in project and XML-style files where the repository already does so.
- Keep constants in `UPPER_SNAKE_CASE`.
- Favor readable, testable methods over thin wrapper methods that do not add clarity.
- Match the existing file layout and avoid unrelated reformatting.

## Testing Expectations

When code changes are involved:

- Add or update tests when behavior changes.
- Use MSTest and Moq.
- Prefer deterministic tests that verify observable behavior.
- Cover edge cases and error handling, not only happy paths.
- Name tests clearly, such as `MethodName_StateUnderTest_ExpectedBehavior`.

For documentation-only changes, tests are usually not required unless the documentation describes behavior that also needs validation.

## Documentation Updates

Update documentation when your change affects setup, architecture, workflows, configuration, or user expectations.

- Use repo-relative paths in documentation.
- Keep root documentation aligned with `/README.md`.
- Update `/docs/DocumentationManifest.md` when documentation inside `/docs` is added, moved, renamed, or substantially revised.

## Pull Requests

Before opening a pull request:

- Rebuild and run the relevant existing tests for your change.
- Keep the pull request focused on one logical change.
- Include any related documentation updates.
- Summarize what changed, why it changed, and how it was validated.
- Call out limitations, follow-up work, or environment-specific assumptions.

By participating in this project, you agree to follow the repository's [Code of Conduct](./CODE_OF_CONDUCT.md).
