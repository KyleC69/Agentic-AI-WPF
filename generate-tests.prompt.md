---
agent: 'agent'
description: 'DEPRECATED: use .github/prompts/generate_unit_tests.prompt.md as the canonical unit test generation prompt'
---

# Deprecated prompt

This prompt file is **deprecated** and is kept only for backward compatibility with existing
tooling or workflows that still reference `generate-tests.prompt.md`.

The **canonical** and up-to-date unit test generation prompt is:

- `.github/prompts/generate_unit_tests.prompt.md`

All changes to test generation behavior, strategies, and guidelines **must** be made in the
canonical prompt file listed above. Do not update this file with prompt logic or instructions.

If you are configuring a new workflow or agent, point it directly at
`.github/prompts/generate_unit_tests.prompt.md` instead of this file.
