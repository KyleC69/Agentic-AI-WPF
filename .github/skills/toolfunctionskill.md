---
name: ToolFunctions.Skill.md
description: Defines the patterns and guidelines to use while creating AI Tool Functions.
appliesto:
---

# ✅ **Skill File: ToolFunctions.Skill.md**

## **Skill: ToolFunctions**

This skill defines the architectural, safety, and behavioral rules for all tool entry points implemented under the `\ToolFunctions` namespace.  
All tools must follow these conventions to ensure deterministic behavior, safe execution, and model‑friendly error surfaces.

---

## **1. Result Contract Requirements**

All tool entry points must return a `ToolResult<T>` using the following rules:

- **Success**
  - Use `ToolResult<T>.Ok(...)`
  - Include structured, concise results
  - Include any metadata that helps the model reason about next steps

- **Failure**
  - Use `ToolResult<T>.Fail(...)`
  - Provide a clear, model‑helpful explanation of:
    - what input was invalid  
    - what boundary was violated  
    - what condition prevented execution  
  - Never throw exceptions out of the tool entry point

- **Exception Handling**
  - Catch all expected exceptions
  - Convert them into `ToolResult<T>.Fail(...)`
  - Include the underlying cause when known

---

## **2. Path‑Bound Tool Rules**

Tools that operate on files or directories must follow strict boundary enforcement:

- Accept at least one allowlist root from `DataIngestionLib.Boundaries`
- Do **not** inspect or validate paths inside the tool entry method
- All path normalization, validation, and boundary enforcement must be routed through:
  - `ToolFunctions/Utils/PathResolver.cs`
- Relative paths must resolve within the configured allowlisted roots
- Attempts to access paths outside allowed roots must return:
  - `ToolResult<T>.Fail("Path outside allowed boundaries", ...)`

Tools must never access the filesystem directly without going through the resolver.

---

## **3. Entry‑Point Shape**

All tool entry points must follow these conventions:

- Add a meaningful `[Description(...)]` attribute to the method
- Add `[Description(...)]` attributes to each public parameter
- Prefer small DTOs for results when metadata helps the model reason, such as:
  - `FullPath`
  - `AllowedRoot`
  - `ItemCount`
  - `Size`
  - `Exists`
  - `IsDirectory`

Tools should expose only the minimum parameters needed for deterministic behavior.

---

## **4. Output Hygiene**

All textual output returned to the model must be cleaned:

- Use `DiagnosticsText.CleanModelText(...)` for:
  - raw command output  
  - logs  
  - file contents  
  - system text that may contain control characters  
- Remove:
  - ANSI codes  
  - null bytes  
  - excessive whitespace  
  - terminal formatting artifacts  

Returned payloads must be:

- concise  
- structured  
- predictable  
- free of noise  

---

## **5. Dependency Pattern**

Tools must follow a consistent dependency model:

- Prefer constructor‑provided configuration (e.g., allowlisted roots)
- Use safe defaults from `DataIngestionLib.Boundaries` when touching the filesystem
- The WPF project acts as the **composition root**
  - All tool instances must be registered there
  - No tool should instantiate its own dependencies internally

Tools must remain deterministic, testable, and free of hidden side effects.

---

## **6. Behavioral Guarantees**

All tools under `\ToolFunctions` must adhere to the following guarantees:

- **Never throw** out of the entry point  
- **Never mutate system state** unless explicitly designed for that purpose  
- **Never access paths outside allowlisted roots**  
- **Never return unstructured or noisy text**  
- **Always return a valid `ToolResult<T>`**  
- **Always provide model‑helpful failure messages**  
- **Always sanitize output**  

---

## **7. Model‑Facing Design Principles**

Tools must be designed to help the model reason effectively:

- Keep operations small and atomic  
- Prefer explicit parameters over implicit behavior  
- Avoid ambiguous or multi‑purpose entry points  
- Provide clear descriptions of:
  - what the tool does  
  - what inputs it accepts  
  - what boundaries apply  
  - what results it returns  

Tools should be predictable, safe, and easy for the model to call correctly.

