# Tool creation pattern

## Required result contract

- Every tool entry point returns `ToolResult<T>`.
- Success responses use `ToolResult<T>.Ok(...)`.
- Failure responses use `ToolResult<T>.Fail(...)` with a model-helpful message that explains what input or boundary caused the failure.
- Tool entry points should catch expected exceptions and convert them into `ToolResult<T>` failures.

## Path-bound tool rules

- File and directory tools must accept at least one whitelist from `DataIngestionLib.Boundaries`.
- Do not inspect or validate paths inside the tool entry method.
- Route all path normalization and boundary enforcement through `ToolFunctions/Utils/PathResolver.cs`.
- Relative paths should be resolved within the configured allowlisted roots.
- Attempts to access paths outside the configured roots must be returned to the model as a failed `ToolResult<T>`.

## Entry-point shape

- Add a meaningful `[Description(...)]` attribute to each tool entry method.
- Add `[Description(...)]` attributes to each public entry parameter.
- Prefer small DTOs for results when metadata such as `FullPath`, `AllowedRoot`, or counts helps the model reason about the next step.

## Output hygiene

- Clean raw text before returning it to the model.
- Use `DiagnosticsText.CleanModelText(...)` for raw text payloads that may contain control characters or unnecessary whitespace noise.
- Keep returned payloads concise and structured.

## Dependency pattern

- Prefer constructor-provided configuration such as allowlisted roots.
- Provide safe defaults from the appropriate class in `DataIngestionLib.Boundaries` when the tool touches filesystem
- Keep the WPF project as the composition root and register tool instances there.
