---
name: SQL Review Runbook
description: Run a standardized SQL review with configurable scope, risk level, and auto-fix mode.
argument-hint: scope=<path|object-set>; risk=<low|medium|high>; autofix=<on|off>; target=<database-name-optional>
agent: SQL Database Review Agent
---

Run a SQL review using this parameter contract:

- scope: required, target area such as sql/AIDataRag, sql/ChatHistory, stored procedures only, triggers only, or a comma-separated list.
- risk: optional, low, medium, or high. Default medium.
- autofix: optional, on or off. Default on.
- target: optional, local database name for read-only validation queries.

Execution rules:

1. Inventory all scripts in scope and identify dependencies.
2. Review correctness, reliability, security, and performance risks.
3. If target is provided, run safe read-only validation queries.
4. If autofix is on, apply low-risk fixes only and add concise SQL object comments where logic is non-obvious.
5. If autofix is off, provide patch-ready edits without applying them.
6. Update sql/README.md when script behavior, dependencies, or execution order changed.

Risk handling:

- low: only no-behavior-change fixes and comments.
- medium: low-risk hardening and bounded reliability fixes.
- high: include broader structural fixes only when explicitly justified and safe.

Required output:

1. Findings ordered by severity.
2. Files and SQL objects reviewed.
3. Changes applied or proposed.
4. Validation checks performed.
5. Remaining risks and assumptions.

If arguments are missing, infer safe defaults and state them at the top of the response.
