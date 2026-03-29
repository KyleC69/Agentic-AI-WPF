
## Repository Architecture Guidelines
---
description: Enforces cohesive, maintainable patterns and prevents AI from generating fragmented, over‑abstracted code.
applyTo: "**/*.cs"
---

# Architectural Principles

- Prefer **cohesion over fragmentation**.  
  A feature or operation should live in **one file** unless there is a clear, demonstrated need for reuse.
  - Prefer base class abstraction over multi-file fragmentation

- Avoid unnecessary layers.  
  Do **not** generate:
  - trivial wrapper classes  
  - empty interfaces  
  - pass‑through services  
  - factories that only call `new`  
  - runners/orchestrators with no logic  
  - micro‑files containing a single 5‑line method  

- Only introduce an interface when:
  - multiple implementations exist **today**, or  
  - the interface expresses a meaningful contract, not ceremony.

- Keep SQL operations **localized**.  
  A single SQL query should not be spread across multiple files unless the logic is genuinely complex.

- Group related logic together.  
  If two classes only exist to support one operation, they belong in the same file or the same cohesive module.

- Avoid “enterprise maximalism.”  
  Do **not** generate:
  - Onion Architecture layers  
  - DDD aggregates/entities/value objects unless explicitly requested  
  - service/repository/manager/runner stacks for simple operations  

# Code Generation Rules

- Prefer **extension methods** over trivial wrapper classes.
- Prefer **static helpers** over factories when no state is required.
- Prefer **records** or simple DTOs over verbose classes when appropriate.
- Keep method names and class names **descriptive and minimal**.
- Group helpers together by concept.

# File Organization

- A file should represent a **concept**, not a pattern.
- Do not split a concept across multiple files unless:
  - the file exceeds ~300 lines, or  
  - the concept has multiple reusable components.

- When generating new files:
  - ensure they contain meaningful logic  
  - avoid scattering related logic across the project  

# Debuggability & Flow

- Generated code must be easy to trace.  
  Avoid deep call chains where each layer only forwards parameters.

- Keep the execution flow **obvious**:
  - minimal indirection  
  - minimal wrappers  
  - clear entry points  

# RAG & Agent Framework Specific Rules

- When generating code for agents:
  - avoid unnecessary agent wrappers  
  - avoid creating “runner” classes unless they contain real orchestration logic  
  - keep context providers, history providers, and injectors **clearly separated**  
  - do not create multiple files for trivial agent operations  

- When generating ingestion or retrieval code:
  - keep parsing, cleaning, and chunking logic cohesive  
  - avoid splitting ingestion steps into micro‑files  

# General Behavior

- Prefer **clarity over ceremony**.  
- Prefer **readability over pattern purity**.  
- Prefer **maintainability over abstraction**.  
- Only generate complexity when the problem demands it.



