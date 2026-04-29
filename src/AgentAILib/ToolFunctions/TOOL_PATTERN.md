# Tool Creation Pattern (Updated for Single‑Wrap Registration Model)

## Required result contract

- Every tool entry point returns `ToolResult<T>`.
- Success responses use `ToolResult<T>.Ok(...)`.
- Failure responses use `ToolResult<T>.Fail(...)` with a clear explanation of what input, boundary, or constraint caused the failure.
- Tool entry points must catch expected exceptions and convert them into `ToolResult<T>.Fail(...)`.

---

## Path‑bound tool rules

- File and directory tools must accept at least one allowlisted root from `AgentAILib.Boundaries`.
- Do **not** perform path validation inside the tool entry method.
- All path normalization and boundary enforcement must go through `ToolFunctions/Utils/PathResolver.cs`.
- Relative paths must resolve within the configured allowlisted roots.
- Attempts to access paths outside the allowlisted boundaries must return a failed `ToolResult<T>` describing the violation.

---

## Entry‑point shape

- Each public tool entry method must have a meaningful `[Description(...)]` attribute.
- Each public parameter must also have a `[Description(...)]` attribute.
- Each entry method represents **one deterministic action** with **no optional parameters** exposed to the model.

---

## Output hygiene

- Clean raw text before returning it to the model.
- Use `DiagnosticsText.CleanModelText(...)` for any raw text payloads that may contain control characters or formatting noise.
- Keep returned payloads concise, structured, and free of irrelevant data.

---

## Dependency pattern

- Prefer constructor‑injected configuration (e.g., allowlisted roots, environment settings).
- Provide safe defaults from `AgentAILib.Boundaries` when a tool interacts with the filesystem.
- The WPF project remains the composition root; all tool instances are created and registered there.

---

## Tool registration rules (updated)

- **Each tool function must be wrapped exactly once** using `AIFunctionFactory.Create(...)` in the `ToolBuilder` constructor.
- Store each wrapped function in a dedicated field.
- `GetReadOnlyAiTools()` and `GetWritingAiTools()` must return **only the stored AITool fields**, never new instances.
- Multi‑function tools (e.g., PsInfoTool, NetStatTool, HandleTool) must expose each action as a **separate, pre‑parameterized method**, each wrapped once into its own `AITool`.
- Single‑function tools (e.g., FileContentsReadingTool, InstalledUpdatesTool) are wrapped once and stored as a single field.
- No `AIFunctionFactory.Create(...)` calls may appear inside `GetReadOnlyAiTools()` or `GetWritingAiTools()`.

This ensures:
- deterministic tool identity  
- stable metadata  
- predictable model behavior  
- no registration drift  
- easier debugging  
