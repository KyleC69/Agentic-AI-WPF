---
name: SQL Audit Read-Only Runbook
description: Run a non-mutating SQL audit with configurable scope and validation depth.
argument-hint: scope=<path|object-set>; validation=<none|scripts|db>; target=<database-name-optional>; focus=<security|reliability|correctness|performance|all>
agent: SQL Audit Read-Only Agent
---

Run a read-only SQL audit using this parameter contract:

- scope: required, target area such as sql/AIDataRag, sql/ChatHistory, stored procedures only, triggers only, or a comma-separated list.
- validation: optional, none, scripts, or db. Default scripts.
- target: optional, local database name for read-only validation queries when validation=db.
- focus: optional, security, reliability, correctness, performance, or all. Default all.

Execution rules:

1. Inventory scripts and objects in scope.
2. Review against the selected focus areas.
3. If validation=db and target is provided, run only safe read-only SQL checks.
4. Do not edit files or apply schema/data changes.
5. Provide patch-ready recommendations and suggested markdown updates.

Required output:

1. Findings ordered by severity.
2. Files and SQL objects reviewed.
3. Validation performed and query classes used.
4. Patch-ready fixes and suggested SQL comments.
5. Suggested updates for sql/README.md.
6. Residual risks and assumptions.

If arguments are missing, infer safe defaults and list the resolved values before findings.
