---
name: SQL Security And Reliability Guidelines
description: "Use when editing SQL scripts in this repo to enforce secure, reliable, and reviewable database changes."
applyTo: "sql/**/*.sql"
---

# SQL Security And Reliability Guidelines

- Prefer idempotent deployment patterns where practical, especially for create operations.
- Avoid dynamic SQL unless absolutely required; if required, parameterize inputs and validate allowlists.
- Never embed secrets, credentials, or tokens in SQL scripts.
- Keep object-level logic deterministic where ordering matters; specify explicit ORDER BY for externally consumed ordered results.
- Bound expensive queries with explicit filtering, top limits, or pagination when feasible.
- Prefer set-based logic over row-by-row patterns unless correctness requires procedural behavior.
- Use explicit transaction boundaries for multi-step write operations that must succeed atomically.
- Add defensive null and edge-case handling for externally supplied inputs.
- Preserve least-privilege assumptions: do not widen permissions without clear need and documentation.
- For non-obvious behavior, add concise inline object comments documenting purpose, assumptions, and safeguards.
- When object behavior changes, update sql/README.md with execution-order and dependency impact.
- For destructive changes, require explicit confirmation and a rollback-aware plan.
