
# AI MANDATORY Constraints

- AI Agents must treat the project file (`.csproj`) and solution file (`.sln`) as the absolute boundary of visibility.
- AI Agents must not read, reference, infer, or operate on any file or folder not included in a project.
- AI must not generate or use wildcard paths that could match files outside the project boundary.
- AI must not use GitHub APIs, repository‑wide scans, or any external service to list or inspect files.
- AI must not attempt to access, compare, or modify any file outside the project’s included items.
- AI must not perform any git operations or reference git state.
- AI must operate **only** on files explicitly included in the project structure.

# Project Guidelines

## Architecture Philosophy

This codebase values **clarity, cohesion, and debuggability** over ceremony or pattern maximalism.

AI generation must follow these principles:

- A file represents a **concept**, not a pattern.
- Abstractions must **earn their existence** by adding clarity or reuse.
- Avoid unnecessary layers, wrappers, and interfaces.
- Keep execution flow **traceable** and **human‑readable**.
- Prefer simplicity over enterprise boilerplate.
- Patterns serve the codebase — not the other way around.

## Architecture

- Treat `src/RAGDataIngestionWPF` as the WPF composition root: host startup, DI registration, navigation, and view orchestration belong there.
- Keep AI, RAG, chat history, ingestion, and tool execution logic in `src/DataIngestionLib`; new behavior there should remain UI-agnostic.
- Keep shared UI-supporting infrastructure in `src/RAGDataIngestionWPF.Core` rather than pushing it into the WPF project or the agent library.
- Start documentation lookup at [docs/DocumentationManifest.md](../docs/DocumentationManifest.md), then use [docs/Architecture.md](../docs/Architecture.md), [docs/ContextManagement.md](../docs/ContextManagement.md), and [docs/Components.md](../docs/Components.md) as needed.

## Build and Test

- Use Windows with the .NET 10 preview SDK; this repo targets `net10.0-windows` and `net10.0-windows10.0.19041.0`.
- Build the WPF app with `dotnet build src/RAGDataIngestionWPF/RAGDataIngestionWPF.csproj`.
- Build the core library with `dotnet build src/DataIngestionLib/DataIngestionLib.csproj`.
- Run tests with `dotnet test tests/RAGDataIngestionWPF.Tests.MSTest/RAGDataIngestionWPF.Tests.MSTest.csproj`.
- The app assumes Windows-specific APIs, local SQL Server configuration, and a local Ollama setup. These should be centrally located to allow simple swapping of implementation to enable other environments. This app is intended as a Windows only application, so cross-platform support is not a requirement.
- If the WPF project fails with transient generated-file errors such as missing `obj/*.g.cs` or `CS5001`, clean `src/RAGDataIngestionWPF/obj` and `bin` and rebuild before changing code.

## Unit Testing

- Use MSTest and Moq for unit testing.
- Tests should be focused on deterministic expected behavior and not generated around implementation details.
- Internals are exposed to tests via `InternalsVisibleTo` in the project file to allow individual testing.
- Tests should target a method and not a chain of calls. If a method is just a one-liner that wraps another call, consider whether it adds enough clarity or reuse to justify its existence.
- Tests should cover edge cases and error handling, not just the happy path. For example, if a method is supposed to throw an exception when given invalid input, there should be a test that verifies that behavior.
- Tests should cover input and output behavior, not just side effects. For example, if a method is supposed to return a certain value based on its input, there should be a test that verifies that behavior, even if the method also has side effects.
- Tests should avoid relying on implementation details or internal state. Instead, they should focus on the observable behavior of the method under test. For example, if a method is supposed to update a database record, the test should verify that the record was updated correctly, rather than checking internal variables or state changes.
- Tests should be named clearly to indicate what behavior they are verifying. A common convention is to use the format `MethodName_StateUnderTest_ExpectedBehavior`, such as `CalculateTotal_WhenGivenValidItems_ReturnsCorrectTotal`.

## Code Style

- Follow `.editorconfig`: use 4-space indentation not tabs for code and project files, and preserve CRLF line endings.
- Match the existing file layout and blank-line-heavy style; avoid mass reformatting unrelated files.
- Prefer constructor injection, `Guard.ThrowIfNull(...)`, async APIs with Cancellation support linked to application lifetime and passed through.
- Keep constants in `UPPER_SNAKE_CASE`. Methods should be testable units of behavior, not just single lines. Avoid one-liner methods that just wrap another call without adding clarity or reuse.
- Methods should be marked internal for testability unless they are part of a public API or need to be public for another reason; Entry points for a class should be public, but helper methods can be internal. Avoid public methods that are only used internally.

## Conventions

- For startup and registration changes, update the Generic Host wiring in `src/RAGDataIngestionWPF/App.xaml.cs` and keep the WPF project as the composition root.
- For chat and context features, preserve the repository's separation between permanent SQL history, semi-volatile conversation cache, durable progress logs, and RAG sources; see [docs/ContextManagement.md](../docs/ContextManagement.md).
- Use `IUserIdentityProvider` for runtime user identity in conversation paths; do not introduce new direct `Environment.UserName` reads there.
- Any configurable settings should be exposed to the user through the Settings page in the UI and grouped together in a GroupBox with a clear label; avoid hidden or hardcoded configuration values.
- **Ignore** any files that are not included in project files either implicitly or explicitly;
- Tests use MSTest and Moq. Favor deterministic behavior and edge-case coverage, and remember that `EventHandler<T>` mocks need both sender and payload when raised.
- The repo already contains reusable custom agents in `.github/agents` and prompts in `.github/prompts`; prefer those when they match the task instead of duplicating their purpose in ad hoc instructions.
- Expose private methods to tests by changing their access modifier to internal for better testing coverage.